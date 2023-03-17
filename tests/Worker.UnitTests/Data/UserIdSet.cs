using System.Linq;

namespace Worker.UnitTests.Data;

internal sealed class UserIdSet
{
    public int InitialUsersAmount { get; }
    public int[] InitialUserIds { get; }
    public string[] InitialUserStringIds { get; }
    public int[] NewUserIds { get; }
    public string[] NewUserStringIds { get; }
    public int[] NewAndExistingUserIds { get; }
    public string[] NewAndExistingUserStringIds { get; }
    public int[] ChangedExistingUserIds { get; }
    public string[] ChangedExistingUserStringIds { get; }

    private UserIdSet(int initialUsersAmount, int subluistAmountDivider)
    {
        InitialUsersAmount = initialUsersAmount;
        var newUsersAmount = InitialUsersAmount / subluistAmountDivider;
        var existingUsersAmountWhenAddNew = newUsersAmount / subluistAmountDivider;
        var changedUsersAmount = InitialUsersAmount / subluistAmountDivider / 2;

        InitialUserIds = Enumerable.Range(1, initialUsersAmount).ToArray();
        NewUserIds = Enumerable.Range(initialUsersAmount + 1, newUsersAmount).ToArray();
        NewAndExistingUserIds = InitialUserIds.Take(existingUsersAmountWhenAddNew).Union(NewUserIds).ToArray();
        ChangedExistingUserIds = Enumerable.Range(1, changedUsersAmount).ToArray();

        InitialUserStringIds = InitialUserIds.Select(x => x.ToString()).ToArray();
        NewUserStringIds = NewUserIds.Select(x => x.ToString()).ToArray();
        NewAndExistingUserStringIds = NewAndExistingUserIds.Select(x => x.ToString()).ToArray();
        ChangedExistingUserStringIds = ChangedExistingUserIds.Select(x => x.ToString()).ToArray();
    }

    public static UserIdSet Create(int initialUsersAmount, int sublistAmountDivider = 10)
        => new(initialUsersAmount, sublistAmountDivider);
}