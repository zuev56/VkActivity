using VkActivity.Worker.Abstractions;
using VkActivity.Worker.Models.VkApi;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace VkActivity.Worker.Services;

internal sealed class VkIntegration : IVkIntegration
{
    // https://dev.vk.com/reference/objects/user
    private const string BaseUrl = "https://api.vk.com/method/";
    private readonly string _getUsersUrl;
    private readonly string _getFriendsUrl;
    private const string FieldsForGettingUserActivity = "online,last_seen";
    private const string FieldsForGettingFullUserInfo = "activities,about,books,bdate,career,connections,contacts,city,country," +
        "domain,education,exports,has_photo,has_mobile,home_town,photo_50,sex,site,schools,screen_name,verified,games,interests," +
        "maiden_name,military,movies,music,nickname,occupation,personal,quotes,relation,relatives,timezone,tv,universities";

    private static readonly SemaphoreSlim _semaphore = new(1, 32);
    private static readonly TimeSpan _apiAccessTimeout = TimeSpan.FromSeconds(3);
    private static DateTime _lastApiAccessTime = DateTime.UtcNow;

    private readonly HttpClient _httpClient = new();
    public static readonly TimeSpan ApiAccessMinInterval = TimeSpan.FromSeconds(0.35);

    public VkIntegration(string token, string version)
    {
        ArgumentNullException.ThrowIfNull(nameof(token));
        ArgumentNullException.ThrowIfNull(nameof(version));

        _getUsersUrl = $"{BaseUrl}users.get?access_token={token}&v={version}&lang=ru";
        _getFriendsUrl = $"{BaseUrl}friends.get?access_token={token}&v={version}&lang=ru";
    }

    public async Task<List<VkApiUser>> GetUsersWithActivityInfoAsync(string[] userScreenNames)
    {
        ArgumentNullException.ThrowIfNull(userScreenNames);

        if (userScreenNames.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userScreenNames));

        var url = $"{_getUsersUrl}&fields={FieldsForGettingUserActivity}&user_ids={string.Join(',', userScreenNames)}";

        return await GetVkUsersAsync(url);
    }

    private async Task<List<VkApiUser>> GetVkUsersAsync(string url)
    {
        var responseResult = await GetResponseAsync<UsersApiResponse>(url).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value?.Users == null)
            throw new InvalidOperationException("Unable to get correct response from Vk API");

        return responseResult.Value.Users;
    }

    private async Task<IOperationResult<TResponse>> GetResponseAsync<TResponse>(string url)
    {
        if (await _semaphore.WaitAsync(_apiAccessTimeout))
        {
            try
            {
                if (DateTime.UtcNow.Subtract(_lastApiAccessTime) < ApiAccessMinInterval)
                    await Task.Delay(ApiAccessMinInterval).ConfigureAwait(false);

                var response = await _httpClient.GetAsync<TResponse>(url).ConfigureAwait(false);

                _lastApiAccessTime = DateTime.UtcNow;

                return ServiceResult<TResponse>.Success(response);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        else
        {
            return ServiceResult<TResponse>.Error("VK API access timeout error");
        }
    }

    public async Task<List<VkApiUser>> GetUsersWithFullInfoAsync(string[] userScreenNames)
    {
        ArgumentNullException.ThrowIfNull(userScreenNames);

        if (userScreenNames.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userScreenNames));

        var url = $"{_getUsersUrl}&fields={FieldsForGettingFullUserInfo}&user_ids={string.Join(',', userScreenNames)}";

        return await GetVkUsersAsync(url);
    }

    public async Task<int[]> GetFriendIds(int userId)
    {
        var url = $"{_getFriendsUrl}&user_id={userId}";

        var responseResult = await GetResponseAsync<FriendsApiResponse>(url).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value?.Data?.FriendIds == null)
            throw new InvalidOperationException("Unable to get correct response from Vk API");

        return responseResult.Value.Data.FriendIds;
    }
}
