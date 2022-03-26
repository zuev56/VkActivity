using VkActivity.Data.Models;

namespace Home.Data.Abstractions;

public interface IVkUsersRepository
{
    Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take);
    Task<List<User>> FindAllAsync(params int[] userIds);
    Task<bool> SaveRangeAsync(IEnumerable<User> usersForSave);
}
