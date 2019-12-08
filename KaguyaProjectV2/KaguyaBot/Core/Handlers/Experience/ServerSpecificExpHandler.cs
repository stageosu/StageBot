﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using KaguyaProjectV2.KaguyaBot.Core.DataStorage.JsonStorage;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Handlers.Experience
{
    public class ServerSpecificExpHandler
    {
        public static async void AddExp(User user, Server server, ICommandContext context)
        {
            IEnumerable<ServerExp> _ = await UtilityQueries.GetAllExpForServer(server);
            IEnumerable<ServerExp> specificExps = _.ToList();

            // If the user can receive exp, give them between 5 and 8.
            if (!await CanGetExperience(specificExps, server, user))
            {
                return;
            }

            var levelAnnouncementChannel = await context.Guild.GetChannelAsync(server.LogLevelAnnouncements);
            var userExpObj = specificExps.FirstOrDefault(x => x.UserId == user.Id);

            double oldLevel = ReturnLevel(specificExps, user);

            Random r = new Random();
            int exp = r.Next(5, 8);

            var levelAnnouncementChannel = await context.Guild.GetChannelAsync(server.LogLevelAnnouncements);

            var userExpObj = new ServerExp();

            if (server.ServerExp != null)
            {
                userExpObj = server.ServerExp.FirstOrDefault(x => x.UserId == user.Id);
            }
            else
            {
                userExpObj = new ServerExp
                {
                    ServerId = server.Id,
                    UserId = user.Id,
                    Exp = 0,
                    LatestExp = 0
                };
            }

            var expObject = new ServerExp
            {
                ServerId = server.Id,
                UserId = user.Id,
                Exp = userExpObj?.Exp ?? 0,
                LatestExp = 0
            };

            expObject.Exp += exp;
            expObject.LatestExp = DateTime.Now.ToOADate();
            await ServerQueries.UpdateServerExp(expObject);

            double newLevel = ReturnLevel(server, user);
            await ConsoleLogger.Log($"[Server Exp]: User has received {exp} exp. [Guild: [Name: {context.Guild.Name} | ID: {server.Id}] " +
                                    $"User: [ID: {user.Id}] | New EXP: {expObject.Exp:N0}]", LogLevel.DEBUG);

            double oldLevel = ReturnLevel(server, user);
            if (HasLeveledUp((int)oldLevel, (int)newLevel))
            {
                await ConsoleLogger.Log($"[Server Exp]: User has leveled up! [ID: {user.Id} | Level: {newLevel} | Experience: {user.Experience}]",
                    LogLevel.INFO);

                if (levelAnnouncementChannel != null && levelAnnouncementChannel is IMessageChannel textChannel)
                {
                    await textChannel.SendMessageAsync(embed: LevelUpEmbed(user, server, context));
                    return;
                }

            if (levelAnnouncementChannel != null && levelAnnouncementChannel is IMessageChannel textChannel)
            {
                await textChannel.SendMessageAsync(embed: LevelUpEmbed(user, specificExps, context));
                return;
            }

            await context.Channel.SendMessageAsync(embed: LevelUpEmbed(user, specificExps, context));
        }

        private static async Task<bool> CanGetExperience(IEnumerable<ServerExp> serverExp, Server server, User user)
        {
            try
            {
                return DateTime.Now.AddSeconds(-120).ToOADate() >=
                       serverExp.FirstOrDefault(x => x.UserId == user.Id)?.LatestExp;
            }
            catch (Exception)
            {
                await ServerQueries.AddServerSpecificExpForUser(new ServerExp
                {
                    Exp = 0,
                    LatestExp = 0,
                    ServerId = server.Id,
                    UserId = user.Id
                });
                return true;
            }
        }

        private static double ReturnLevel(IEnumerable<ServerExp> serverExp, User user)
        {
            double? exp = server.ServerExp?.FirstOrDefault(x => x.UserId == user.Id)?.Exp ?? 0;
            return Math.Sqrt((double) exp / 8 + -8);
        }

        private static bool HasLeveledUp(int oldLevel, int newLevel)
        {
            return oldLevel < newLevel;
        }

        public static Embed LevelUpEmbed(User user, IEnumerable<ServerExp> serverExp, ICommandContext context)
        {
            var exp = serverExp.ToList();
            var rankIndex = exp.OrderByDescending(x => x.Exp).ToList().FindIndex(x => x.UserId == user.Id) + 1;

            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder
            {
                Title = "Level Up!",
                Description = $"{context.User.Username} just leveled up! \n" +
                              $"[Server Level: {ReturnLevel(exp, user)} | Experience Points: {user.Experience:N0}]\n" +
                              $"Rank: #{rankIndex}/{exp.Count():N0}",
                ThumbnailUrl = ConfigProperties.client.GetUser(user.Id).GetAvatarUrl()
            };

            return embed.Build();
        }
    }
}
