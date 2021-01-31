﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kaguya.Database.Context;
using Kaguya.Database.Interfaces;
using Kaguya.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Kaguya.Database.Repositories
{
    public class ServerExperienceRepository : IServerExperienceRepository
    {
        private readonly KaguyaDbContext _dbContext;

        public ServerExperienceRepository(KaguyaDbContext dbContext) { _dbContext = dbContext; }

        public async Task<ServerExperience> GetAsync(ulong serverId, ulong userId)
        {
            return await _dbContext.ServerExperience
                                   .AsQueryable()
                                   .Where(x => x.ServerId == serverId && x.UserId == userId)
                                   .FirstOrDefaultAsync();
        }
        
        public async Task<ServerExperience> GetOrCreateAsync(ulong serverId, ulong userId)
        {
            if (await GetAsync(serverId, userId) == null)
            {
                ServerExperience entity = _dbContext.ServerExperience.Add(new ServerExperience
                {
                    ServerId = serverId,
                    UserId = userId,
                    LastGivenExp = null
                }).Entity;

                await _dbContext.SaveChangesAsync();

                return entity;
            }

            return await _dbContext.ServerExperience
                             .AsQueryable()
                             .Where(x => x.ServerId == serverId && x.UserId == userId)
                             .FirstOrDefaultAsync();
        }
        
        public async Task DeleteAsync(ulong serverId, ulong userId)
        {
            var match = await GetAsync(serverId, userId);

            if (match != null)
            {
                _dbContext.ServerExperience.Remove(match);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(ServerExperience value)
        {
            _dbContext.ServerExperience.Update(value);
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertAsync(ServerExperience value)
        {
            _dbContext.ServerExperience.Add(value);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<IList<ServerExperience>> GetAllExpForServer(ulong serverId)
        {
            return await _dbContext.ServerExperience.AsQueryable().Where(x => x.ServerId == serverId).ToListAsync();
        }

        public async Task Add(ulong serverId, ulong userId, int amount)
        {
            ServerExperience match = await GetOrCreateAsync(serverId, userId);
            match.AddExp(amount);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Subtract(ulong serverId, ulong userId, int amount)
        {
            ServerExperience match = await GetOrCreateAsync(serverId, userId);
            match.SubtractExp(amount);
            await _dbContext.SaveChangesAsync();
        }
    }
}