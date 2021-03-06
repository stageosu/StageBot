﻿using Discord;
using Discord.WebSocket;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Internal.Enums;
using Kaguya.Internal.Events;
using Kaguya.Internal.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services.Recurring
{
	public class AntiraidWorker : BackgroundService, ITimerReceiver
	{
		private readonly IAntiraidProcessorInternal _arProcessor;
		private readonly DiscordShardedClient _client;
		private readonly ConcurrentDictionary<ulong, AntiRaidConfig> _configsCache = new();
		private readonly ILogger<AntiraidWorker> _logger;
		private readonly IServiceProvider _provider;
		private readonly SilentSysActions _sysActions;
		private readonly ITimerService _timerService;
		private readonly ConcurrentDictionary<ulong, ConcurrentQueue<(DateTimeOffset userJoinTime, ulong userId)>> _userIdCache = new();

		public AntiraidWorker(ILogger<AntiraidWorker> logger, IServiceProvider provider, ITimerService timerService,
			IAntiraidService arService, DiscordShardedClient client, SilentSysActions sysActions)
		{
			_logger = logger;
			_provider = provider;
			_timerService = timerService;
			_client = client;
			_sysActions = sysActions;
			_arProcessor = (IAntiraidProcessorInternal) arService;
		}

		public async Task HandleTimer(object payload)
		{
			using (var scope = _provider.CreateScope())
			{
				var antiraidConfigRepo = scope.ServiceProvider.GetRequiredService<AntiraidConfigRepository>();

				foreach (var element in _userIdCache)
				{
					if (!_configsCache.TryGetValue(element.Key, out var curConfig))
					{
						var freshConfig = await antiraidConfigRepo.GetAsync(element.Key);

						if (freshConfig != null && freshConfig.ConfigEnabled)
						{
							curConfig = freshConfig;
							_configsCache.TryAdd(freshConfig.ServerId, freshConfig);
						}
						else
						{
							_userIdCache.TryRemove(element);

							continue;
						}
					}

					try
					{
						PruneOldCache(element.Value, curConfig.Seconds);
					}
					catch (Exception e)
					{
						_logger.LogCritical(e, "Anti-raid cache pruning failure");
					}

					if (element.Value.Count == 0)
					{
						_userIdCache.TryRemove(element);
						_configsCache.TryRemove(curConfig.ServerId, out var _);
					}
				}
			}

			// Re-queue for garbage collection every 15 minutes.
			await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddMinutes(15), this);
		}

		// Runs on startup, triggers on user join. Responsible for processing raids.
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// First execution
			await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddHours(1), this);

			while (!stoppingToken.IsCancellationRequested)
			{
				await _arProcessor.GetChannel().Reader.WaitToReadAsync(stoppingToken);

				var data = await _arProcessor.GetChannel().Reader.ReadAsync(stoppingToken);

				if (!_configsCache.TryGetValue(data.ServerId, out var curConfig))
				{
					using (var scope = _provider.CreateScope())
					{
						var antiraidConfigRepo = scope.ServiceProvider.GetRequiredService<AntiraidConfigRepository>();
						curConfig = await antiraidConfigRepo.GetAsync(data.ServerId);

						if (curConfig == null || !curConfig.ConfigEnabled)
						{
							continue;
						}

						_configsCache.TryAdd(curConfig.ServerId, curConfig);
					}
				}

				// Populating caches
				if (!_userIdCache.ContainsKey(data.ServerId))
				{
					var stack = new ConcurrentQueue<(DateTimeOffset userJoinTime, ulong userId)>();
					stack.Enqueue((data.JoinTime, data.UserId));

					_userIdCache.TryAdd(data.ServerId, stack);
				}
				else
				{
					_userIdCache[data.ServerId].Enqueue((data.JoinTime, data.UserId));
				}

				// Antiraid algorithm
				if (!_userIdCache.TryGetValue(data.ServerId, out var curUserCollection) || curUserCollection == null)
				{
					continue;
				}

				try
				{
					PruneOldCache(curUserCollection, curConfig.Seconds);
				}
				catch (Exception e)
				{
					_logger.LogCritical(e, "Anti-raid cache pruning failure");

					throw;
				}

				// Ensure count of users in window is same as current config threshold.
				if (curUserCollection.Count < curConfig.UserThreshold)
				{
					continue;
				}

				_logger.LogInformation($"Antiraid service triggered for guild {data.ServerId}");

				var taskList = new List<Task>();

				using (var scope = _provider.CreateScope())
				{
					var aaRepo = scope.ServiceProvider.GetRequiredService<AdminActionRepository>();
					var ksRepo = scope.ServiceProvider.GetRequiredService<KaguyaServerRepository>();

					var server = await ksRepo.GetOrCreateAsync(curConfig.ServerId);

					if (!server.IsPremium)
					{
						// We deny antiraid protections for non-premium servers.
						continue;
					}

					foreach (ulong userId in curUserCollection.Select(x => x.userId).Distinct())
					{
						var action = curConfig.Action;

						var guild = _client.GetGuild(data.ServerId);
						var user = guild?.GetUser(userId);

						if (user == null)
						{
							continue;
						}

						string reason = "Kaguya Anti-Raid service";

						var adminAction = new AdminAction
						{
							ServerId = data.ServerId,
							ModeratorId = _client.CurrentUser.Id, // Bot ID
							ActionedUserId = data.UserId,
							Action = null,
							Reason = "Automatic server protection (Kaguya Anti-Raid)",
							Expiration = curConfig.PunishmentLength.HasValue
								? DateTimeOffset.Now.Add(curConfig.PunishmentLength.Value)
								: null,
							IsHidden = false,
							IsSystemAction = true,
							Timestamp = DateTimeOffset.Now
						};

						KaguyaEvents.OnAntiraidTrigger(adminAction, user);

						if (curConfig.PunishmentDmEnabled && !string.IsNullOrWhiteSpace(curConfig.AntiraidPunishmentDirectMessage))
						{
							string dmString = curConfig.AntiraidPunishmentDirectMessage;
							dmString = SerializeDmString(dmString, user, action, guild.Name);

							try
							{
								var dmChannel = await user.GetOrCreateDMChannelAsync();
								await dmChannel.SendMessageAsync(dmString);
							}
							catch (Exception e)
							{
								_logger.LogWarning($"Failed to DM user {user.Id} that they were {ActionPastTense(action)} " +
								                   $"as part of an antiraid event in guild {guild.Id}. Error: {e.Message}");
							}
						}

						switch (action)
						{
							case ModerationAction.Mute:
								adminAction.Action = AdminAction.MuteAction;
								taskList.Add(_sysActions.SilentMuteUserAsync(user, server.MuteRoleId));

								break;
							case ModerationAction.Kick:
								adminAction.Action = AdminAction.KickAction;
								taskList.Add(user.KickAsync(reason));

								break;
							case ModerationAction.Shadowban:
								adminAction.Action = AdminAction.ShadowbanAction;
								await _sysActions.SilentShadowbanUserAsync(user, server.ShadowbanRoleId);

								break;
							case ModerationAction.Ban:
								adminAction.Action = AdminAction.BanAction;
								taskList.Add(user.BanAsync(1, reason));

								break;
						}

						await aaRepo.InsertAsync(adminAction);
					}
				}

				await Task.WhenAll(taskList);

				var failures = taskList.Where(x => !x.IsCompletedSuccessfully);
				foreach (var fail in failures)
				{
					_logger.LogError(fail.Exception, $"Antiraid task failure for guild {data.ServerId}.");
				}
			}
		}

		private void PruneOldCache(ConcurrentQueue<(DateTimeOffset userJoinTime, ulong userId)> element, uint windowLength)
		{
			var threshold = DateTimeOffset.Now.AddSeconds(-windowLength);
			while (element.TryPeek(out var userJoinData) && userJoinData.userJoinTime < threshold)
			{
				if (element.TryDequeue(out var _))
				{
					_logger.LogDebug(
						$"Prune occurred with successful dequeue: [User id: {userJoinData.userId} | Join time: {userJoinData.userJoinTime}]");
				}
				else
				{
					_logger.LogError($"Dequeue failed user id: {userJoinData.userId} user join time: {userJoinData.userJoinTime}");

					break;
				}
			}
		}

		private string SerializeDmString(string dmString, SocketUser user, ModerationAction action, string guildName)
		{
			dmString = dmString.Replace("{USERNAME}", user.Username);
			dmString = dmString.Replace("{USERMENTION}", user.Mention);
			dmString = dmString.Replace("{ACTION}", ActionPastTense(action));
			dmString = dmString.Replace("{SERVERNAME}", guildName);

			return dmString;
		}

		private string ActionPastTense(ModerationAction action)
		{
			return action switch
			{
				ModerationAction.Ban => "banned",
				ModerationAction.Kick => "kicked",
				ModerationAction.Mute => "muted",
				ModerationAction.Shadowban => "shadowbanned",
				var _ => "<unknown action>"
			};
		}
	}

	public interface IAntiraidProcessorInternal
	{
		public Channel<AntiraidData> GetChannel();
	}

	public interface IAntiraidService
	{
		public Task TriggerAsync(ulong serverId, ulong userId);
	}

	public class AntiraidService : IAntiraidProcessorInternal, IAntiraidService
	{
		private static readonly Channel<AntiraidData> _antiraidChannel = Channel.CreateUnbounded<AntiraidData>();
		public Channel<AntiraidData> GetChannel() { return _antiraidChannel; }

		public async Task TriggerAsync(ulong serverId, ulong userId)
		{
			await _antiraidChannel.Writer.WriteAsync(new AntiraidData
			{
				ServerId = serverId,
				JoinTime = DateTimeOffset.Now,
				UserId = userId
			});
		}
	}
}