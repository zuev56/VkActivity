﻿using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.VkApi;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace VkActivity.Service.Services;

internal sealed class VkIntegration : IVkIntegration
{
    // https://dev.vk.com/reference/objects/user
    private const string BaseUrl = "https://api.vk.com/method/";
    private readonly string _getUsersUrl;
    private readonly string _getFriendsUrl;
    private const string FieldsForGettingUserActivity = "online,last_seen";
    private const string FieldsForGettingFullUserInfo = "activities,about,blacklisted,blacklisted_by_me,books,bdate,can_be_invited_group,can_post," +
        "can_see_all_posts,can_see_audio,can_send_friend_request,can_write_private_message,career,connections,contacts,city,country,domain,education," +
        "exports,followers_count,friend_status,has_photo,has_mobile,home_town,photo_50,sex,site,schools,screen_name,status,verified,games,interests," +
        "is_favorite,is_friend,is_hidden_from_feed,last_seen,maiden_name,military,movies,music,nickname,occupation,online,personal,quotes,relation," +
        "relatives,timezone,tv,universities";

    // screen_name - короткое имя страницы
    // domain - короткий адрес страницы (например, andrew, id35828305)

    private static readonly SemaphoreSlim _semaphore = new(1, 32);
    private static readonly TimeSpan _apiAccessTimeout = TimeSpan.FromSeconds(3);
    private static DateTime _lastApiAccessTime = DateTime.UtcNow;

    private readonly HttpClient _httpClient = new();
    private static readonly TimeSpan _apiAccessMinInterval = TimeSpan.FromSeconds(0.35);

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
                if (DateTime.UtcNow.Subtract(_lastApiAccessTime) < _apiAccessMinInterval)
                    await Task.Delay(_apiAccessMinInterval).ConfigureAwait(false);

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
