using VkActivity.Data.Models;
using VkActivity.Data.Repositories;
using Zs.Common.Models;

namespace VkActivity.Data.Abstractions;

public interface IUsersRepository : IBaseRepository<User>
{
    Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take, CancellationToken cancellationToken = default);
    Task<List<User>> FindAllByIdsAsync(int[] userIds);
    Task<User?> FindByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<User>> FindAllAsync(int? skip, int? take, CancellationToken cancellationToken = default);
    Task<int[]> FindAllIdsAsync(CancellationToken cancellationToken = default);
    Task<int[]> FindExistingIdsAsync(int[] userIds, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateRangeAsync(IEnumerable<User> users, CancellationToken cancellationToken = default);
}
