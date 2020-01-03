﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Utility
{
    public class Stats : ModuleBase<ShardedCommandContext>
    {
        [UtilityCommand]
        [Command("Stats")]
        [Summary("Returns a set of pretty interesting stats!")]
        [Remarks("")]
        public async Task Command()
        {
            Process curProcess = Process.GetCurrentProcess();

            var client = ConfigProperties.Client;
            var owner = client.GetUser(ConfigProperties.BotConfig.BotOwnerId);

            var curShard = client.GetShardFor(Context.Guild);

            int curTextChannels = 0;
            int curVoiceChannels = 0;
            int curUsers = 0;
            int curOnline = 0;

            foreach (var guild in curShard.Guilds)
            {
                curTextChannels += guild.TextChannels.Count;
                curVoiceChannels += guild.VoiceChannels.Count;
                curUsers += guild.Users.Count;
                curOnline += guild.Users.Count(x => x.Status != UserStatus.Offline);
            }

            int totalGuilds = 0;
            int totalTextChannels = 0;
            int totalVoiceChannels = 0;
            int totalUsers = 0;

            foreach (var shard in client.Shards)
            {
                totalGuilds += shard.Guilds.Count;

                foreach (var guild in shard.Guilds)
                {
                    totalTextChannels += guild.TextChannels.Count;
                    totalVoiceChannels += guild.VoiceChannels.Count;
                    totalUsers += guild.Users.Count;
                }
            }

            var cmdsLastDay = await UtilityQueries.GetCommandHistoryLast24HoursAsync();
            var mostPopCommand = await UtilityQueries.GetMostPopularCommandAsync();

            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Author",
                    Value = $"User: `{owner}`\n" +
                            $"Id: `{owner.Id}`"
                },
                new EmbedFieldBuilder
                {
                    Name = "Command Stats",
                    Value = $"Commands Run (Last 24 Hours): `{cmdsLastDay.Count:N0}`\n" +
                            $"Commands Run (All-time): `{await UtilityQueries.GetTotalCommandCountAsync():N0}`\n" +
                            $"Most Popular Command: `{mostPopCommand.Keys.FirstOrDefault()} with {mostPopCommand.Values.FirstOrDefault()} uses.`"
                },
                new EmbedFieldBuilder
                {
                    Name = "Shard Stats",
                    Value = $"Current Shard: `{curShard.ShardId:N0} / {client.Shards.Count:N0}`\n" +
                            $"Guilds: `{curShard.Guilds.Count:N0}`\n" +
                            $"Text Channels: `{curTextChannels:N0}`\n" +
                            $"Voice Channels: `{curVoiceChannels:N0}`\n" +
                            $"Total Users: `{curUsers:N0}`\n" +
                            $"Online Users: `{curOnline:N0}`\n" +
                            $"Latency: `{curShard.Latency:N0}ms`\n"
                },
                new EmbedFieldBuilder
                {
                    Name = "Global Stats",
                    Value = $"Uptime: `{(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize()}`\n" +
                            $"Guilds: `{totalGuilds:N0}`\n" +
                            $"Text Channels: `{totalTextChannels:N0}`\n" +
                            $"Voice Channels: `{totalVoiceChannels:N0}`\n" +
                            $"Users: `{totalUsers:N0}`\n" +
                            $"RAM Usage: `{(double)curProcess.PrivateMemorySize64 / 1000000:N2} Megabytes`"
                },
                new EmbedFieldBuilder
                {
                    Name = "Kaguya User Stats",
                    Value = $"Registered Users: `{await UtilityQueries.GetCountOfUsersAsync():N0}`\n" +
                            $"Total Points in Circulation: `{UtilityQueries.GetTotalCurrency():N0}`\n" +
                            $"Total Gambles: `{await UtilityQueries.GetTotalGamblesAsync():N0}`"
                }
            };

            var embed = new KaguyaEmbedBuilder
            {
                Title = "Kaguya Statistics",
                Fields = fields
            };
            embed.SetColor(EmbedColor.GOLD);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
