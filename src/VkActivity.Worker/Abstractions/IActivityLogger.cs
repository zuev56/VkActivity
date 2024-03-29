﻿using System.Threading.Tasks;
using Zs.Common.Models;

namespace VkActivity.Worker.Abstractions;

public interface IActivityLogger
{
    Task<Result> SaveUsersActivityAsync();
    Task<Result> ChangeAllUserActivitiesToUndefinedAsync();
}