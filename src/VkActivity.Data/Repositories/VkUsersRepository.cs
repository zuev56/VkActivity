using Microsoft.EntityFrameworkCore;
using VkActivity.Data.Models;

namespace Home.Data.Repositories;

public class VkUsersRepository
{
    public VkUsersRepository()
    {
    }

    public async Task<List<User>> FindAllWhereNameLikeValueAsync(string value, int? skip, int? take)
    {
        return await FindAllAsync(u => EF.Functions.ILike(u.FirstName, $"%{value}%") || EF.Functions.ILike(u.LastName, $"%{value}%"), skip: skip, take: take);
    }

    private Task<List<User>> FindAllAsync(Func<User, bool> p, int? skip, int? take)
    {
        throw new NotImplementedException();
    }
}
