﻿using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogServices;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers
{
    public static class AutoUnmuteHandler
    {
        public static Task Initialize()
        {
            var timer = new Timer(5000)
            {
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += async (sender, e) =>
            {
                List<MutedUser> curMutedUsers = await DatabaseQueries.GetAllAsync<MutedUser>(x => x.ExpiresAt < DateTime.Now.ToOADate());
                foreach (MutedUser mutedUser in curMutedUsers)
                {
                    SocketGuild guild = ConfigProperties.Client.GetGuild(mutedUser.ServerId);

                    if (guild == null)
                        goto RemoveFromDB;

                    Server server = await DatabaseQueries.GetOrCreateServerAsync(guild.Id);
                    SocketGuildUser user = ConfigProperties.Client.GetGuild(server.ServerId).GetUser(mutedUser.UserId);

                    if (server.IsPremium)
                    {
                        await PremiumModerationLog.SendModerationLog(new PremiumModerationLog
                        {
                            Server = server,
                            Moderator = ConfigProperties.Client.GetGuild(server.ServerId)
                                                        .GetUser(538910393918160916),
                            ActionRecipient = user,
                            Action = PremiumModActionHandler.UNMUTE,
                            Reason = "User was automatically unmuted because their timed mute has expired."
                        });
                    }

                    try
                    {
                        SocketRole muteRole = guild.Roles.FirstOrDefault(x => x.Name == "kaguya-mute");
                        await user.RemoveRoleAsync(muteRole);
                    }
                    catch (Exception)
                    {
                        await ConsoleLogger.LogAsync($"Exception handled when unmuting a user in guild [Name: {guild.Name} | ID: {guild.Id}]",
                            LogLvl.WARN);
                    }

                    await DatabaseQueries.UpdateAsync(server);

                    RemoveFromDB:
                    await DatabaseQueries.DeleteAsync(mutedUser);
                    await ConsoleLogger.LogAsync($"User [ID: {mutedUser.UserId}] has been automatically unmuted.",
                        LogLvl.DEBUG);
                }
            };

            return Task.CompletedTask;
        }
    }
}