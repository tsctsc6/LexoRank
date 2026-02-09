using LexoRank.Core;

namespace LexoRank.Test;

public class InsertBetween
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(CommonCharacterSets.Digits);
        var sortedList = new SortedDictionary<string, int>();
        var sortedValue0 = lexoRankManager.Between(string.Empty, string.Empty).Unwrap();
        sortedList.Add(sortedValue0, 0);
        var sortedValue1 = lexoRankManager.Between(sortedValue0, string.Empty).Unwrap();
        sortedList.Add(sortedValue1, 1);
        var sortedValue2 = lexoRankManager.Between(sortedValue0, sortedValue1).Unwrap();
        sortedList.Add(sortedValue2, 2);
        var sortedArray = sortedList.ToArray();
        Assert.Equal(0, sortedArray[0].Value);
        Assert.Equal(2, sortedArray[1].Value);
        Assert.Equal(1, sortedArray[2].Value);
    }
}
