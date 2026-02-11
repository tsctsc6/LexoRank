using LexoRank.Core;

namespace LexoRank.Test;

public class InsertMuti
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(CommonCharacterSets.Digits);
        var sortedList = new SortedDictionary<string, int>();
        var sortedValue = string.Empty;
        for (var i = 0; i < 100; i++)
        {
            sortedValue = lexoRankManager.Between(sortedValue, string.Empty);
            sortedList.Add(sortedValue, i);
        }
        var sortedArray = sortedList.ToArray();
        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(i, sortedArray[i].Value);
        }
    }
}
