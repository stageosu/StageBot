﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using KaguyaProjectV2.KaguyaBot.Core.Services.ConsoleLogService;
using KaguyaProjectV2.KaguyaBot.DataStorage.JsonStorage;
using System;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class Kick : KaguyaBase
    {
        [AdminCommand]
        [Command("Kick")]
        [Alias("k")]
        [Summary("Kicks a user, or a list of users, from the server.")]
        [Remarks("<user>\n<user> {...}")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(SocketGuildUser user, [Remainder]string reason = null)
        {
            KaguyaEmbedBuilder embed = new KaguyaEmbedBuilder();

            reason ??= "<No reason provided>";
            
            try
            {
                await user.KickAsync(reason);
                embed.Description = $"Successfully kicked `{user}` with reason `{reason}`";
            }
            catch (Exception e)
            {
                embed.Description = $"Failed to kick `{user}`.";
            }

            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        /// Silently kick a user. This should only be executed by the WarnHandler class.
        /// </summary>
        /// <param name="user">The user to kick.</param>
        /// <param name="reason">The reason for kicking the user.</param>
        /// <returns></returns>
        public async Task AutoKickUserAsync(SocketGuildUser user, string reason)
        {
            try
            {
                await user.KickAsync(reason);
                await ConsoleLogger.LogAsync($"User auto-kicked. Guild: [Name: {user.Guild.Name} | ID: {user.Guild.Id}] " +
                                             $"User: [Name: {user} | ID: {user.Id}]", LogLvl.DEBUG);
            }
            catch (Exception e)
            {
                await ConsoleLogger.LogAsync($"Attempt to auto-kick user has failed in guild " +
                                        $"[{user.Guild.Name} | {user.Guild.Id}]. Exception: {e.Message}", LogLvl.INFO);
            }
        }
    }
}
