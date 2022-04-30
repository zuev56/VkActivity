using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.VkApi;
using Zs.Common.Services.WebAPI;

namespace VkActivity.Service.Services;

internal sealed class VkIntegration : IVkIntegration
{
    // https://dev.vk.com/reference/objects/user
    private readonly string _getUsersUrl;
    private const string FIELDS_FOR_GETTING_ACTIVITY = "online,online_mobile,online_app,last_seen";
    private const string FIELDS_FOR_ADDING_USER = "photo_id,verified,sex,bdate,city,country,home_town,photo_max_orig,online,domain,has_mobile,"
                    + "contacts,site,education,universities,schools,status,last_seen,followers_count,occupation,nickname,relatives,"
                    + "relation,personal,connections,exports,activities,interests,music,movies,tv,books,games,about,quotes,can_post,"
                    + "can_see_all_posts,can_see_audio,can_write_private_message,can_send_friend_request,is_favorite,is_hidden_from_feed,"
                    + "timezone,screen_name,maiden_name,is_friend,friend_status,career,military,blacklisted,blacklisted_by_me,can_be_invited_group";

    //NEW: activities,about,blacklisted,blacklisted_by_me,books,bdate,can_be_invited_group,can_post,can_see_all_posts,can_see_audio,can_send_friend_request,can_write_private_message,career,connections,contacts,city,country,domain,education,exports,followers_count,friend_status,has_photo,has_mobile,home_town,photo_50,sex,site,schools,screen_name,status,verified,games,interests,is_favorite,is_friend,is_hidden_from_feed,last_seen,maiden_name,military,movies,music,nickname,occupation,online,personal,quotes,relation,relatives,timezone,tv,universities




    // photo_50 - мини фото
    // screen_name - короткое имя страницы
    // domain - короткий адрес страницы (например, andrew, id35828305)
    // first_name_{case}, last_name_{case}, где case = падеж
    // last_seen.time, last_seen.platform 
    //     1 — мобильная версия;
    //     2 — приложение для iPhone;
    //     3 — приложение для iPad;
    //     4 — приложение для Android;
    //     5 — приложение для Windows Phone;
    //     6 — приложение для Windows 10;
    //     7 — полная версия сайта.

    public VkIntegration(string token, string version)
    {
        ArgumentNullException.ThrowIfNull(nameof(token));
        ArgumentNullException.ThrowIfNull(nameof(version));

        _getUsersUrl = $"https://api.vk.com/method/users.get?access_token={token}&v={version}&lang=ru";
    }

    public async Task<List<VkApiUser>> GetUsersWithActivityInfoAsync(int[] userIds)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        if (userIds.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userIds));

        var url = $"{_getUsersUrl}&fields={FIELDS_FOR_GETTING_ACTIVITY}&user_ids={string.Join(',', userIds)}";

        return await GetVkUsersAsync(url);
    }

    private static async Task<List<VkApiUser>> GetVkUsersAsync(string url)
    {
        var response = await ApiHelper.GetAsync<VkApiResponse>(url, throwExceptionOnError: true);

        if (response?.Users == null)
            throw new InvalidOperationException("Unable to get correct response from Vk API");

        return response.Users;
    }

    public async Task<List<VkApiUser>> GetUsersWithFullInfoAsync(int[] userIds)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        if (userIds.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userIds));

        var url = $"{_getUsersUrl}&fields={FIELDS_FOR_ADDING_USER}&user_ids={string.Join(',', userIds)}";

        return await GetVkUsersAsync(url);
    }
}
