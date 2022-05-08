using System.Linq;

namespace UnitTests.Data;

internal class UserIdSet
{
    public int InitialUsersAmount { get; }
    public int[] InitialUserIds { get; }
    public string[] InitialUserStringIds { get; }
    public int[] NewUserIds { get; }
    public string[] NewUserStringIds { get; }
    public int[] NewAndExistingUserIds { get; }
    public string[] NewAndExistingUserStringIds { get; }
    public int[] ChangedUserIds { get; }

    private UserIdSet(int initialUsersAmount, int subluistAmountDivider)
    {
        InitialUsersAmount = initialUsersAmount;
        int newUsersAmount = InitialUsersAmount / subluistAmountDivider;
        int existingUsersAmountWhenAddNew = newUsersAmount / subluistAmountDivider;

        InitialUserIds = Enumerable.Range(1, initialUsersAmount).ToArray();
        InitialUserStringIds = InitialUserIds.Select(x => x.ToString()).ToArray();
        NewUserIds = Enumerable.Range(initialUsersAmount + 1, newUsersAmount).ToArray();
        NewUserStringIds = NewUserIds.Select(x => x.ToString()).ToArray();
        NewAndExistingUserIds = InitialUserIds.Take(existingUsersAmountWhenAddNew).Union(NewUserIds).ToArray();
        NewAndExistingUserStringIds = NewAndExistingUserIds.Select(x => x.ToString()).ToArray();
        ChangedUserIds = NewUserIds;
    }

    public static UserIdSet Create(int initialUsersAmount, int subluistAmountDivider = 10)
        => new UserIdSet(initialUsersAmount, subluistAmountDivider);
}
