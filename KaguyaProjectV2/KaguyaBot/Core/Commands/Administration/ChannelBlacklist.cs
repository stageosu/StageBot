﻿using Discord.Commands;
using Humanizer;
using KaguyaProjectV2.KaguyaBot.Core.Attributes;
using KaguyaProjectV2.KaguyaBot.Core.Exceptions;
using KaguyaProjectV2.KaguyaBot.Core.Extensions;
using KaguyaProjectV2.KaguyaBot.Core.Global;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Models;
using KaguyaProjectV2.KaguyaBot.DataStorage.DbData.Queries;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.Administration
{
    public class ChannelBlacklist : ModuleBase<ShardedCommandContext>
    {
        [AdminCommand]
        [Command("ChannelBlacklist")]
        [Alias("cbl")]
        [Summary("Makes a channel blacklisted, disabling Kaguya completely in the channel. " +
                 "Kaguya will not respond to commands or " +
                 "post level-up announcements in this channel. Users with the `Administrator` server permission " +
                 "override this blacklist, and will not notice its effect. Level-up announcements will, however, " +
                 "still be disabled even if they are an `Administrator`.\n\n" +
                 "**Arguments:**\n\n" +
                 "The `-r` argument may be passed to unblacklist a channel (either the current channel or the specified one).\n" +
                 "The `-all` argument may be passed to completely disable Kaguya in the entire server.\n" +
                 "The `-clear` argument may be passed to lift all existing blacklists.\n" +
                 "The `-t` argument may be used to specify a time, and can be added to the other arguments as well! " +
                 "Seconds, minutes, hours, and even days may be passed in as an argument.\n\n" +
                 "To blacklist a specific channel, pass in its `ID`. If you don't know the ID, " +
                 "you can simply use this command without an argument to " +
                 "blacklist the channel this command was executed from.")]
        [Remarks("(<= blacklists current channel)\n<ID> (<= blacklists a specific channel)\n" +
                 "-r (<= Un-blacklists the current channel)\n" +
                 "-r <ID> (<= Un-blacklists the specified channel)." +
                 "\n-all (<= blacklists all channels)\n" +
                 "-clear(<= removes all blacklists)\n" +
                 "-t 35m (<= blacklists current channel for 35 minutes)\n-all -t 12d36h (<= blacklists " +
                 "all channels for 12 days and 36 hours.)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Command(params string[] _)
        {
            var args = _.ToList();
            var server = await DatabaseQueries.GetOrCreateServerAsync(Context.Guild.Id);
            var currentBlacklists = server.BlackListedChannels.ToList();

            var hasT = false;
            var hasAll = false;

            var expiration = DateTime.MaxValue.ToOADate();
            var expirationString = "This blacklist will never expire.";

            if (args.Any(x => x.ToLower().Contains("-r")))
            {
                if (args.Count > 2)
                {
                    goto ArgumentProcessException;
                }
                if(args.Count == 1)
                {
                    var curUnblacklist = await DatabaseQueries.GetAllAsync<BlackListedChannel>(x =>
                        x.ChannelId == Context.Channel.Id && x.ServerId == Context.Guild.Id);
                    await DatabaseQueries.DeleteAsync(curUnblacklist);

                    await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully unblacklisted channel " +
                                                                     $"`{Context.Channel.Name}`");
                    return;
                }
                if (args.Count == 2)
                {
                    var toUnblacklistChannel = Context.Guild.GetTextChannel(args[1].AsUlong());
                    var curUnblacklist = await DatabaseQueries.GetAllAsync<BlackListedChannel>(x =>
                        x.ChannelId == toUnblacklistChannel.Id && x.ServerId == Context.Guild.Id);
                    await DatabaseQueries.DeleteAsync(curUnblacklist);

                    await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully unblacklisted channel " +
                                                                     $"`{Context.Channel.Name}`");
                    return;
                }
            }

            if (args.Any(x => x.ToLower().Contains("-t")))
            {
                var tIndex = args.FindIndex(x => x.ToLower().Contains("-t"));
                var ts = TimeSpan.MaxValue;
                try
                {
                    ts = args[tIndex + 1].ParseToTimespan();
                }
                catch (IndexOutOfRangeException)
                {
                    await Context.Channel.SendBasicErrorEmbedAsync($"Please specify a time after the `-t` argument.");
                }
                catch (Exception)
                {
                    throw new KaguyaSupportException("An error occurred when parsing the specified timespan. Either " +
                                                     "the timespan was too long, too short, or an invalid argument " +
                                                     "was passed.");
                }

                // Lib will reply in chat if any other exceptions occur, such as an invalid time input or too long, etc.

                if (ts.TotalDays > 365)
                    throw new InvalidOperationException("The duration value may not be over 365 days.");
                if (ts.TotalSeconds < 5)
                    throw new InvalidOperationException("The duration value must be at least 5.");

                expiration = (DateTime.Now + ts).ToOADate();
                hasT = true;

                // We have this twice because tIndex will be "0" both times after the first one is removed.
                // $cbl -t 30m => Index 0 is removed, indexoutofrange ex. thrown for tIndex + 1 (which is now 0).
                expirationString = expiration == DateTime.MaxValue.ToOADate()
                    ? "This will last until cancelled."
                    : $"This blacklist will expire in `{(DateTime.FromOADate(expiration) - DateTime.Now).Humanize()}`";

                args.RemoveAt(tIndex);
                args.RemoveAt(tIndex);
            }

            // Since we set the "time args" to null above, we can now do normal checks :)
            if (args.Any(x => x.ToLower().Contains("-all")))
            {
                if(args.Count > 1)
                    goto ArgumentProcessException;

                await DatabaseQueries.DeleteAllForServerAsync<BlackListedChannel>(server.ServerId);
                var allIndex = args.FindIndex(x => x.ToLower().Contains("-all"));
                args.RemoveAt(allIndex);
                hasAll = true;
            }

            if (!hasAll && args.Any(x => x.ToLower().Contains("-clear")))
            {
                if (args.Count > 1)
                    goto ArgumentProcessException;

                await DatabaseQueries.DeleteAllForServerAsync<BlackListedChannel>(server.ServerId);
                await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully cleared `{currentBlacklists.Count}` " +
                                                                 $"channels from the blacklist.");
                return;
            }

            if (args.Count == 1) // See if the arg is actually a channel in this guild
            {
                if (args.Any(x => x.Contains("-")))
                {
                    goto ArgumentProcessException;
                }
                try
                {
                    SocketTextChannel channel;
                    try
                    {
                        channel = Context.Guild.GetTextChannel(args[0].AsUlong());
                    }
                    catch(Exception)
                    {
                        try
                        {
                            // Replaces the <#> in <#94580295820586> (some random ID)
                            args[0] = args[0].Replace("<#", "").Replace(">", "");
                            channel = Context.Guild.GetTextChannel(args[0].AsUlong());
                        }
                        catch(Exception)
                        {
                            throw new KaguyaSupportException("I was unable to parse this input as a valid channel ID.");
                        }
                    }


                    var cbl = new BlackListedChannel
                    {
                        ServerId = Context.Guild.Id,
                        ChannelId = channel.Id,
                        Expiration = expiration
                    };

                    await Context.Channel.SendBasicSuccessEmbedAsync($"Successfully blacklisted channel `{channel}`. " +
                                                                     $"{expirationString}");

                    await DatabaseQueries.InsertAsync(cbl);
                    return;
                }
                catch (NullReferenceException)
                {
                    throw new KaguyaSupportException($"The specified channel ID does not exist in this server.");
                }
            }

            if (args.Count == 0)
            {
                if (hasAll)
                {
                    foreach (var channel in Context.Guild.Channels)
                    {
                        var cbl = new BlackListedChannel
                        {
                            ServerId = Context.Guild.Id,
                            ChannelId = channel.Id,
                            Expiration = expiration
                        };

                        await DatabaseQueries.InsertAsync(cbl);
                    }

                    await Context.Channel.SendBasicSuccessEmbedAsync(
                        $"Successfully blacklisted `{Context.Guild.Channels.Count}` " +
                        $"channels. {expirationString}");
                    return;
                }
                else
                {
                    var cbl = new BlackListedChannel
                    {
                        ServerId = Context.Guild.Id,
                        ChannelId = Context.Channel.Id,
                        Expiration = expiration
                    };

                    await DatabaseQueries.InsertAsync(cbl);
                    await Context.Channel.SendBasicSuccessEmbedAsync(
                        $"Successfully blacklisted this channel. {expirationString}");
                    return;
                }
            }

            ArgumentProcessException:
            throw new KaguyaSupportException("The specified arguments were unable to be processed.");
        }
    }
}