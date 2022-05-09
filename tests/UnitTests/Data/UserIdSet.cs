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
    public string[] ChangedUserStringIds { get; }

    private UserIdSet(int initialUsersAmount, int subluistAmountDivider)
    {
        InitialUsersAmount = initialUsersAmount;
        int newUsersAmount = InitialUsersAmount / subluistAmountDivider;
        int existingUsersAmountWhenAddNew = newUsersAmount / subluistAmountDivider;
        int changedUsersAmount = InitialUsersAmount / subluistAmountDivider / 2;

        InitialUserIds = Enumerable.Range(1, initialUsersAmount).ToArray();
        NewUserIds = Enumerable.Range(initialUsersAmount + 1, newUsersAmount).ToArray();
        NewAndExistingUserIds = InitialUserIds.Take(existingUsersAmountWhenAddNew).Union(NewUserIds).ToArray();
        ChangedUserIds = Enumerable.Range(1, changedUsersAmount).ToArray();

        InitialUserStringIds = InitialUserIds.Select(x => x.ToString()).ToArray();
        NewUserStringIds = NewUserIds.Select(x => x.ToString()).ToArray();
        NewAndExistingUserStringIds = NewAndExistingUserIds.Select(x => x.ToString()).ToArray();
        ChangedUserStringIds = ChangedUserIds.Select(x => x.ToString()).ToArray();
    }

    public static UserIdSet Create(int initialUsersAmount, int subluistAmountDivider = 10)
        => new(initialUsersAmount, subluistAmountDivider);
}
