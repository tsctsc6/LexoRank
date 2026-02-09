using LexoRank.Core;

namespace LexoRank.Test;

public class InsertPrev
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager("0123456789");
        var sortedList = new SortedDictionary<string, int>();
        var sortedValue0 = lexoRankManager.Between(string.Empty, string.Empty).Unwrap();
        sortedList.Add(sortedValue0, 0);
        var sortedValue1 = lexoRankManager.Between(string.Empty, sortedValue0).Unwrap();
        sortedList.Add(sortedValue1, 1);
        var sortedArray = sortedList.ToArray();
        Assert.Equal(1, sortedArray[0].Value);
        Assert.Equal(0, sortedArray[1].Value);
    }
}
