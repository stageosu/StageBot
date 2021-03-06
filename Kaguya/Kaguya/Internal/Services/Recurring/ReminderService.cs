﻿using Discord;
using Discord.WebSocket;
using Kaguya.Database.Repositories;
using Kaguya.Discord;
using Kaguya.Internal.Extensions.DiscordExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kaguya.Internal.Services.Recurring
{
	public class ReminderService : BackgroundService, ITimerReceiver
	{
		// We have this flag to combat duplicate reminders when they are spam created.
		private static bool _processingReminders;
		private readonly DiscordShardedClient _client;
		private readonly ILogger<ReminderService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly ITimerService _timerService;

		public ReminderService(ILogger<ReminderService> logger, IServiceProvider serviceProvider, ITimerService timerService,
			DiscordShardedClient client)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_timerService = timerService;
			_client = client;
		}

		public async Task HandleTimer(object payload)
		{
			await _timerService.TriggerAtAsync(DateTimeOffset.Now.AddSeconds(5), this);

			if (_processingReminders)
			{
				return;
			}

			if (!_client.AllShardsReady())
			{
				_logger.LogDebug("Shards not ready, aborting");
				return;
			}

			using (var scope = _serviceProvider.CreateScope())
			{
				_processingReminders = true;
				var reminderRepository = scope.ServiceProvider.GetRequiredService<ReminderRepository>();
				var reminders = await reminderRepository.GetAllToDeliverAsync();

				foreach (var reminder in reminders)
				{
					var socketUser = _client.GetUser(reminder.UserId);
					if (socketUser == null)
					{
						_logger.LogWarning($"Could not find {reminder.UserId}");

						continue;
					}

					var reminderEmbed = new KaguyaEmbedBuilder(KaguyaColors.Tan)
						{
							Description = "🗒️ Kaguya Reminders".AsBold(),
							Fields = new List<EmbedFieldBuilder>
							{
								new()
								{
									Name = "Message",
									Value = reminder.Text
								}
							}
						}.WithCurrentTimestamp()
						 .Build();

					try
					{
						var dmChannel = await socketUser.GetOrCreateDMChannelAsync();
						await dmChannel.SendMessageAsync(embed: reminderEmbed);
					}
					catch (Exception e)
					{
						_logger.LogWarning(e,
							$"Failed to message user {reminder.UserId} their reminder notification. " + $"Remind Id: {reminder.Id}");
					}
					finally
					{
						reminder.HasTriggered = true;
						await reminderRepository.UpdateAsync(reminder);

						_logger.LogInformation($"Sent reminder to user {socketUser.Id} to '{reminder.Text}'");
					}
				}

				_processingReminders = false;
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			await _timerService.TriggerAtAsync(DateTimeOffset.Now, this);
		}
	}
}