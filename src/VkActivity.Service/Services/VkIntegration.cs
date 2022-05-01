using VkActivity.Service.Abstractions;
using VkActivity.Service.Models.VkApi;
using Zs.Common.Services.WebAPI;

namespace VkActivity.Service.Services;

internal sealed class VkIntegration : IVkIntegration
{
    // https://dev.vk.com/reference/objects/user
    private readonly string _getUsersUrl;
    private const string FieldsForGettingUserActivity = "online,last_seen";
    private const string FieldsForGettingFullUserInfo = "activities,about,blacklisted,blacklisted_by_me,books,bdate,can_be_invited_group,can_post," +
        "can_see_all_posts,can_see_audio,can_send_friend_request,can_write_private_message,career,connections,contacts,city,country,domain,education," +
        "exports,followers_count,friend_status,has_photo,has_mobile,home_town,photo_50,sex,site,schools,screen_name,status,verified,games,interests," +
        "is_favorite,is_friend,is_hidden_from_feed,last_seen,maiden_name,military,movies,music,nickname,occupation,online,personal,quotes,relation," +
        "relatives,timezone,tv,universities";

    // screen_name - короткое имя страницы
    // domain - короткий адрес страницы (например, andrew, id35828305)

    public VkIntegration(string token, string version)
    {
        ArgumentNullException.ThrowIfNull(nameof(token));
        ArgumentNullException.ThrowIfNull(nameof(version));

        _getUsersUrl = $"https://api.vk.com/method/users.get?access_token={token}&v={version}&lang=ru";
    }

    public async Task<List<VkApiUser>> GetUsersWithActivityInfoAsync(string[] userScreenNames)
    {
        ArgumentNullException.ThrowIfNull(userScreenNames);

        if (userScreenNames.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userScreenNames));

        var url = $"{_getUsersUrl}&fields={FieldsForGettingUserActivity}&user_ids={string.Join(',', userScreenNames)}";

        return await GetVkUsersAsync(url);
    }

    private static async Task<List<VkApiUser>> GetVkUsersAsync(string url)
    {
        var response = await ApiHelper.GetAsync<VkApiResponse>(url, throwExceptionOnError: true);

        if (response?.Users == null)
            throw new InvalidOperationException("Unable to get correct response from Vk API");

        return response.Users;
    }

    public async Task<List<VkApiUser>> GetUsersWithFullInfoAsync(string[] userScreenNames)
    {
        ArgumentNullException.ThrowIfNull(userScreenNames);

        if (userScreenNames.Length == 0)
            throw new ArgumentException("UserIds array couldn't be empty", nameof(userScreenNames));

        var url = $"{_getUsersUrl}&fields={FieldsForGettingFullUserInfo}&user_ids={string.Join(',', userScreenNames)}";

        return await GetVkUsersAsync(url);
    }
}
