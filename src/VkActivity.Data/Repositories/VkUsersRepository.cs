﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VkActivity.Data;
using VkActivity.Data.Models;
using VkActivity.Data.Repositories;

namespace Home.Data.Repositories;

public sealed class UsersRepository : BaseRepository<VkActivityContext, User>
{
    public UsersRepository(
        IDbContextFactory<VkActivityContext> contextFactory,
        TimeSpan? criticalQueryExecutionTimeForLogging = null,
        ILoggerFactory? loggerfFactory = null)
        : base(contextFactory, criticalQueryExecutionTimeForLogging, loggerfFactory)
    {
    }

    public async Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take)
    {
        return await FindAllAsync(u => EF.Functions.ILike(u.FirstName, $"%{value}%") || EF.Functions.ILike(u.LastName, $"%{value}%"), skip: skip, take: take);
    }

}
