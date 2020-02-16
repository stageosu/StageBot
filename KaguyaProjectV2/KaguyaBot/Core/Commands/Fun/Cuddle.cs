﻿using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.Core.KaguyaEmbed;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Fun
{
    public class Cuddle : KaguyaBase
    {
        [FunCommand]
        [Command("Cuddle")]
        [Summary("Cuddle somebody, or multiple people!")]
        [Remarks("<user> {...}")]
        public async Task Command(params SocketGuildUser[] users)
        {
            var cuddleGif = await ConfigProperties.NekoClient.Action_v3.CuddleGif();

            if (users.Length == 1)
            {
                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Cuddle *uwu*",
                    Description = $"{Context.User.Mention} cuddled {users[0].Mention}!",
                    ImageUrl = cuddleGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
                return;
            }
            else
            {
                var names = new List<string>();
                users.ToList().ForEach(x => names.Add(x.Mention));

                var embed = new KaguyaEmbedBuilder
                {
                    Title = "Cuddle *uwu*",
                    Description = $"{Context.User.Mention} cuddled {names.Humanize()}!",
                    ImageUrl = cuddleGif.ImageUrl
                };

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}