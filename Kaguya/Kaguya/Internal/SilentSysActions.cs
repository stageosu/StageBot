﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Kaguya.Discord.Commands.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kaguya.Internal
{
    /// <summary>
    /// Used for silent operations (mute, shadowban) during automated "user punishment" processes
    /// such as the <see cref="Kaguya.Internal.Services.AntiraidWorker"/>.
    /// </summary>
    public class SilentSysActions
    {
        private readonly ILogger<SilentSysActions> _logger;

        public SilentSysActions(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<SilentSysActions>>();
        }

        public async Task<bool> SilentMuteUserAsync(SocketGuildUser user, ulong? muteRoleId)
        {
            return await SilentApplyRoleAsync(user, muteRoleId, true);
        }

        public async Task<bool> SilentShadowbanUserAsync(SocketGuildUser user, ulong? muteRoleId)
        {
            return await SilentApplyRoleAsync(user, muteRoleId, false);
        }

        /// <summary>
        /// Applys either a mute role or shadowban role to the user. If not mute, it's a shadowban.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleId"></param>
        /// <param name="mute">Whether this is a mute role. If false, applys shadowban role.</param>
        /// <returns>Whether the mute role was successfully applied to the user and whether
        /// Kaguya could update all guild text channels with <see cref="OverwritePermissions"/> for the role.</returns>
        private async Task<bool> SilentApplyRoleAsync(SocketGuildUser user, ulong? roleId, bool mute)
        {
            var guild = user.Guild;
            IRole role = roleId.HasValue ? guild.GetRole(roleId.Value) : null;

            if (role == null)
            {
                string roleName = mute ? "kaguya-mute" : "kaguya-shadowban";

                if (guild.Roles.Any(x => x.Name.Equals(roleName)))
                {
                    role = guild.Roles.FirstOrDefault(x => x.Name == roleName);
                }

                if (role == null)
                {
                    IRole newRole = await CreateRoleAsync(guild, roleName, GuildPermissions.None);
                    if (newRole == null)
                    {
                        return false;
                    }

                    role = newRole;
                }
            }

            try
            {
                await user.AddRoleAsync(role);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception encountered during processing of sys-action automated mute for " +
                                    $"user {user.Id} in guild {guild.Id}.");

                return false;
            }

            if (guild.Channels.Any(x => !x.GetPermissionOverwrite(role).HasValue))
            {
                OverwritePermissions owPerms = Mute.GetMuteOverwritePermissions();
                
                try
                {
                    foreach (var channel in guild.Channels.Where(x => !x.GetPermissionOverwrite(role).HasValue))
                    {
                        if (!mute)
                        {
                            owPerms = OverwritePermissions.DenyAll(channel);
                        }
                        
                        await channel.AddPermissionOverwriteAsync(role, owPerms);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception encountered during the updating of the text channel " +
                                        $"permissions in guild {guild.Id} for role {role.Name}.");

                    return false;
                }
            }

            return true;
        }

        private static async Task<IRole> CreateRoleAsync(SocketGuild guild, string name, GuildPermissions permissions)
        {
            try
            {
                RestRole role = await guild.CreateRoleAsync(name, permissions, null, false, null);

                return role;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}