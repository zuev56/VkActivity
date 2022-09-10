using System.Collections.Concurrent;
using VkActivity.Api.Abstractions;
using VkActivity.Api.Models;
using VkActivity.Data.Abstractions;
using VkActivity.Data.Models;
using Zs.Common.Abstractions;
using Zs.Common.Extensions;
using Zs.Common.Models;
using static VkActivity.Api.Models.Constants;

namespace VkActivity.Api.Services;

public sealed class ActivityAnalyzer : IActivityAnalyzer
{
    private readonly IActivityLogItemsRepository _vkActivityLogRepo;
    private readonly IUsersRepository _vkUsersRepo;
    private readonly ILogger<ActivityAnalyzer> _logger;
    private readonly DateTime _tmpMinLogDate = new(2020, 10, 01);

    public ActivityAnalyzer(
        IActivityLogItemsRepository vkActivityLogRepo,
        IUsersRepository vkUsersRepo,
        ILogger<ActivityAnalyzer> logger)
    {
        _vkActivityLogRepo = vkActivityLogRepo ?? throw new ArgumentNullException(nameof(vkActivityLogRepo));
        _vkUsersRepo = vkUsersRepo ?? throw new ArgumentNullException(nameof(vkUsersRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <inheritdoc/>
    public async Task<IOperationResult<DetailedActivity>> GetFullTimeActivityAsync(int userId)
    {
        try
        {
            var user = await _vkUsersRepo.FindByIdAsync(userId);
            if (user == null)
                return ServiceResult<DetailedActivity>.Error(UserNotFound(userId));

            var orderedLogForUser = await GetOrderedLog(_tmpMinLogDate, DateTime.UtcNow, userId);
            if (!orderedLogForUser.Any())
                return ServiceResult<DetailedActivity>.Warning(ActivityForUserNotFound(userId), new DetailedActivity(user));

            var activityDetails = new DetailedActivity(user)
            {
                AnalyzedDaysCount = (int)(orderedLogForUser.Max(l => l.InsertDate.Date) - orderedLogForUser.Min(l => l.InsertDate.Date)).TotalDays,
                ActivityDaysCount = orderedLogForUser.Select(l => l.InsertDate.Date).Distinct().Count(),
                VisitsFromSite = orderedLogForUser.Count(l => l.IsOnline == true && IsWebSite(l.Platform)),
                VisitsFromApp = orderedLogForUser.Count(l => l.IsOnline == true && !IsWebSite(l.Platform)),
                //ActivityCalendar  = GetActivityForEveryDay(orderedLogForUser), Пока не используется
                TimeInSite = TimeSpan.FromSeconds(GetActivitySeconds(orderedLogForUser.ToList(), Device.PC)),
                TimeInApp = TimeSpan.FromSeconds(GetActivitySeconds(orderedLogForUser.ToList(), Device.Mobile)),
                TimeOnPlatforms = GetTimeOnPlatforms(orderedLogForUser),
            };

            return ServiceResult<DetailedActivity>.Success(activityDetails);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, GetFullTimeActivityError);
            return ServiceResult<DetailedActivity>.Error(GetFullTimeActivityError);
        }
    }

    private static bool IsWebSite(Platform platform)
        => platform == Platform.MobileSiteVersion || platform == Platform.FullSiteVersion;


    /// <inheritdoc/>
    public async Task<IOperationResult<List<ActivityListItem>>> GetUsersWithActivityAsync(string? filterText, DateTime fromDate, DateTime toDate)
    {
        try
        {
            if (fromDate >= toDate)
                return ServiceResult<List<ActivityListItem>>.Error(EndDateIsNotMoreThanStartDate);

            var usersResult = await GetUsersAsync(filterText);
            if (!usersResult.IsSuccess)
                return ServiceResult<List<ActivityListItem>>.ErrorFrom(usersResult);

            var activityLog = await GetOrderedLog(fromDate, toDate, usersResult.Value.Select(u => u.Id).ToArray());
            var onlineUserIds = await GetOnlineUserIdsAsync();

            var userActivityBag = new ConcurrentBag<ActivityListItem>();
            usersResult.Value.AsParallel().ForAll(user =>
            {
                var orderedLogForUser = activityLog.Where(l => l.UserId == user.Id).OrderBy(l => l.LastSeen).ToList();

                AddToLogClosingIntervalItem(orderedLogForUser, toDate);

                userActivityBag.Add(new ActivityListItem(user)
                {
                    //ActivitySec = GetActivitySeconds(orderedLogForUser, Device.All),
                    ActivitySec = (int)GetTimeOnPlatforms(orderedLogForUser).Sum(i => i.Value.TotalSeconds),
                    IsOnline = onlineUserIds.Any(id => id == user.Id)
                });
            });

            var orderedUserList = userActivityBag
                .OrderByDescending(i => i.ActivitySec).ThenBy(i => i.User.FirstName).ThenBy(i => i.User.LastName)
                .ToList();

            return ServiceResult<List<ActivityListItem>>.Success(orderedUserList);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, GetUsersWithActivityError);
            return ServiceResult<List<ActivityListItem>>.Error(GetUsersWithActivityError);
        }
    }

    //public async Task<IOperationResult<Table<UserWithActivity>>> GetUsersWithActivityTable(TableParameters tableParameters)
    //{
    //    try
    //    {
    //        if (tableParameters == null)
    //            throw new ArgumentNullException(nameof(tableParameters));
    //
    //        int userCount = await _vkUsersRepo.CountAsync();
    //        int skip = tableParameters.Paging.CurrentPage * tableParameters.Paging.RecordsOnPage;
    //        int take = tableParameters.Paging.RecordsOnPage;
    //
    //        var usersResult = await GetUsers(tableParameters.FilterText, skip, take);
    //
    //        var log = await GetOrderedLog(tableParameters.FromDate, tableParameters.ToDate, usersResult.Value.Select(u => u.Id).ToArray());
    //        var onlineUserIds = await GetOnlineUserIdsAsync();
    //
    //        var userActivityBag = new ConcurrentBag<UserWithActivity>();
    //        usersResult.Value.AsParallel().ForAll(user =>
    //        {
    //            var orderedLog = log.Where(l => l.UserId == user.Id).OrderBy(l => l.LastSeen).ToList();
    //
    //            AddToLogClosingIntervalItem(orderedLog, tableParameters.ToDate);
    //
    //            userActivityBag.Add(new UserWithActivity
    //            {
    //                User = user,
    //                ActivitySec = GetActivitySeconds(orderedLog, Device.All),
    //                isOnline = onlineUserIds.Any(id => id == user.Id)
    //            });
    //        });
    //
    //        var orderedUserList = userActivityBag
    //            .OrderByDescending(i => i.ActivitySec).ThenBy(i => i.User.FirstName).ThenBy(i => i.User.LastName)
    //            .ToList();
    //
    //        return ServiceResult<List<UserWithActivity>>.Success(orderedUserList);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger?.LogError(ex, "GetVkUsersWithActivity error");
    //        return ServiceResult<List<UserWithActivity>>.Error("Failed to get users list with activity time");
    //    }
    //}

    /// <inheritdoc/>
    public async Task<IOperationResult<List<User>>> GetUsersAsync(string? filterText = null, int? skip = null, int? take = null)
    {
        try
        {
            var users = !string.IsNullOrWhiteSpace(filterText)
                ? await _vkUsersRepo.FindAllWhereNameLikeValueAsync(filterText, skip, take)
                : await _vkUsersRepo.FindAllAsync(skip, take);

            return ServiceResult<List<User>>.Success(users);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, GetUsersError);
            return ServiceResult<List<User>>.Error(GetUsersError);
        }
    }

    private async Task<int[]> GetOnlineUserIdsAsync(params int[] userIds)
    {
        var lastUsersActivity = await _vkActivityLogRepo.FindLastUsersActivityAsync(userIds);

        return lastUsersActivity.Where(i => i.IsOnline == true && DateTime.UtcNow - i.InsertDate < TimeSpan.FromDays(1))
            .Select(i => i.UserId).ToArray();
    }

    private static void AddToLogClosingIntervalItem(List<ActivityLogItem> orderedLog, DateTime toDate)
    {
        // Для корректного отображения времени активности пользователей, которые в данный момент онлайн
        // может потребоваться фейковая запись в журнал
        var lastLogItem = orderedLog.LastOrDefault();
        var endInterval = DateTime.UtcNow < toDate ? DateTime.UtcNow.ToUnixEpoch() : toDate.ToUnixEpoch();
        if (lastLogItem?.IsOnline == true)
            orderedLog.Add(new ActivityLogItem { IsOnline = false, LastSeen = endInterval, InsertDate = DateTime.UtcNow });
    }

    private Dictionary<DateTime, TimeSpan> GetActivityForEveryDay(List<ActivityLogItem> log)
    {
        // Вычисление активности за каждый день должно начинаться с начала суток, если предыдущие сутки закончились онлайн
        if (log == null)
            throw new ArgumentOutOfRangeException(nameof(log));

        var resultMap = new Dictionary<DateTime, TimeSpan>();

        bool prevDayEndedOnlineFromPC = false;
        bool prevDayEndedOnlineFromMobile = false;
        log.Select(l => l.InsertDate.Date).Distinct().ToList().ForEach(day =>
        {
            int secondsADay = 0;
            var dailyLog = log.Where(l => l.InsertDate.Date == day).OrderBy(l => l.InsertDate).ToList();

            if (prevDayEndedOnlineFromPC || prevDayEndedOnlineFromMobile)
                secondsADay += dailyLog[0].LastSeen - day.ToUnixEpoch();

            // TODO: разделить подсчёт времени с браузера и мобильного
            secondsADay += GetActivitySeconds(dailyLog, Device.All);

            // Фиксируем, как закончился предыдущий день
            // TODO: исправить! FromPC - только если Platform.FullSiteVersion. А лучше конкретизировать платформу
            prevDayEndedOnlineFromPC = false;
            prevDayEndedOnlineFromMobile = false;
            var last = dailyLog.Last();
            if (last.IsOnline == true)
            {
                if (!IsWebSite(last.Platform)) prevDayEndedOnlineFromMobile = true;
                else prevDayEndedOnlineFromPC = true;

                secondsADay += (day + TimeSpan.FromDays(1)).ToUnixEpoch() - last.LastSeen;
            }
        });

        return resultMap;
    }

    /// <summary>Gets log and order it</summary>
    private async Task<List<ActivityLogItem>> GetOrderedLog(DateTime fromDate, DateTime toDate, params int[] userIds)
    {
        int fromDateUnix = fromDate.ToUnixEpoch();
        int toDateUnix = toDate.ToUnixEpoch();

        var log = await _vkActivityLogRepo.FindAllByIdsInDateRangeAsync(userIds, fromDate, toDate);

        return log.OrderBy(l => l.LastSeen)
                  .SkipWhile(l => l.IsOnline != true)
                  .ToList();
    }

    /// <summary>Get activity time from list of <see cref="ActivityLogItem"/>s in seconds</summary>
    /// <param name="orderedLog">Ordered list of <see cref="ActivityLogItem"/>s</param>
    /// <param name="device">The device type from which the site was used</param>
    [Obsolete]
    private static int GetActivitySeconds(List<ActivityLogItem> orderedLog, Device device)
    {
        // Проверка:
        //  - Первый элемент списка должен быть IsOnline == true
        //  - Каждый последующий элемент обрабатывается опираясь на предыдущий
        // Обработка ситуаций:
        //  - Предыдущий IsOnline + Mobile  -> Текущий IsOnline + !Mobile
        //  - Предыдущий IsOnline + Mobile  -> Текущий !IsOnline
        //  - Предыдущий IsOnline + !Mobile -> Текущий IsOnline + Mobile
        //  - Предыдущий IsOnline + !Mobile -> Текущий !IsOnline
        //  - Предыдущий !IsOnline          -> Текущий IsOnline + Mobile
        //  - Предыдущий !IsOnline          -> Текущий IsOnline + !Mobile

        // TODO: обработать для каждого типа Platform

        int seconds = 0;
        for (int i = 1; i < orderedLog.Count; i++)
        {
            var prev = orderedLog[i - 1];
            var cur = orderedLog[i];
            var prevIsOnlineMobile = !IsWebSite(prev.Platform);
            var curIsOnlineMobile = !IsWebSite(cur.Platform);

            switch (device)
            {
                case Device.PC:
                    if (prev.IsOnline == true && !prevIsOnlineMobile && (cur.IsOnline == true && curIsOnlineMobile || cur.IsOnline == false))
                        seconds += cur.LastSeen - prev.LastSeen;
                    break;
                case Device.Mobile:
                    if (prev.IsOnline == true && prevIsOnlineMobile && (cur.IsOnline == true && !curIsOnlineMobile || cur.IsOnline == false))
                        seconds += cur.LastSeen - prev.LastSeen;
                    break;
                case Device.All:
                    if (prev.IsOnline == true)
                        seconds += cur.LastSeen - prev.LastSeen;
                    break;
            }
        }

        // Для корректного отображения времени активности пользователей, которые в данный момент онлайн
        // надо прибавлять секунды с момента их входа в Вк до текущего момента (!!!Можно дописывать в журнал фейковую запись о выходе!!!).
        // Но это решение портит результат, когда анализируется отрезок времени, заканчивающийся в прошлом.
        //var lastLogItem = log.LastOrDefault();
        //if (lastLogItem?.IsOnline == true && lastLogItem.InsertDate...)
        //    seconds += DateTime.UtcNow.ToUnixEpoch() - log.Last().InsertDate.ToUnixEpoch();

        return seconds;
    }

    private static int GetActivitySecondsOnPlatform(List<ActivityLogItem> orderedLog, Platform platform)
    {
        // Проверка:
        //  - Первый элемент списка должен быть IsOnline == true
        //  - Каждый последующий элемент обрабатывается опираясь на предыдущий

        int seconds = 0;
        for (int i = 1; i < orderedLog.Count; i++)
        {
            var prev = orderedLog[i - 1];
            var cur = orderedLog[i];

            if (prev.IsOnline == true
                && prev.Platform == platform
                && (cur.IsOnline == false || cur.IsOnline == true && cur.Platform == platform))
            {
                seconds += cur.LastSeen - prev.LastSeen;
            }
        }

        // Для корректного отображения времени активности пользователей, которые в данный момент онлайн
        // надо прибавлять секунды с момента их входа в Вк до текущего момента (!!!Можно дописывать в журнал фейковую запись о выходе!!!).
        // Но это решение портит результат, когда анализируется отрезок времени, заканчивающийся в прошлом.
        //var lastLogItem = log.LastOrDefault();
        //if (lastLogItem?.IsOnline == true && lastLogItem.InsertDate...)
        //    seconds += DateTime.UtcNow.ToUnixEpoch() - log.Last().InsertDate.ToUnixEpoch();

        return seconds;
    }
    public async Task<IOperationResult<SimpleActivity>> GetUserStatisticsForPeriodAsync(int userId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            if (fromDate >= toDate || _tmpMinLogDate >= toDate)
                return ServiceResult<SimpleActivity>.Error(EndDateIsNotMoreThanStartDate);

            fromDate = fromDate > _tmpMinLogDate ? fromDate : _tmpMinLogDate;

            var user = await _vkUsersRepo.FindByIdAsync(userId);
            if (user == null)
                return ServiceResult<SimpleActivity>.Error(UserNotFound(userId));

            var orderedLogForUser = await GetOrderedLog(fromDate, toDate, userId);
            var userStatistics = GetUserStatistics(userId, user.GetFullName(), orderedLogForUser.ToList());

            return ServiceResult<SimpleActivity>.Success(userStatistics);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, GetUserStatisticsForPeriodError);
            return ServiceResult<SimpleActivity>.Error(GetUserStatisticsForPeriodError);
        }
    }

    private SimpleActivity GetUserStatistics(int userId, string userName, List<ActivityLogItem> orderedLogForUser)
    {

        int browserActivitySec = GetActivitySeconds(orderedLogForUser, Device.PC);
        int mobileActivitySec = GetActivitySeconds(orderedLogForUser, Device.Mobile);

        return new SimpleActivity
        {
            UserId = userId,
            UserName = userName,
            TimeInSite = TimeSpan.FromSeconds(browserActivitySec),
            TimeInApp = TimeSpan.FromSeconds(mobileActivitySec),
            TimeOnPlatforms = GetTimeOnPlatforms(orderedLogForUser),
            VisitsCount = orderedLogForUser.Count(l => l.IsOnline == true)
        };
    }

    private Dictionary<Platform, TimeSpan> GetTimeOnPlatforms(List<ActivityLogItem> orderedLogForUser)
    {
        var timeOnPlatforms = new Dictionary<Platform, TimeSpan>();

        foreach (var platform in Enum.GetValues<Platform>())
        {
            var seconds = GetActivitySecondsOnPlatform(orderedLogForUser, platform);

            if (seconds > 0)
                timeOnPlatforms.Add(platform, TimeSpan.FromSeconds(seconds));
        }

        return timeOnPlatforms;
    }
}
