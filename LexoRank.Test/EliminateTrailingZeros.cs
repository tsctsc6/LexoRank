using LexoRank.Core;
using LexoRank.Core.Errors;
using RustSharp;

namespace LexoRank.Test;

public class EliminateTrailingZeros
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager("0123456789");
        var sortedValue = lexoRankManager.Between("10721", "29279").Unwrap();
        Assert.Equal("2", sortedValue);
        var sortedList = new SortedDictionary<string, int>
        {
            { "29279", 2 },
            { "10721", 0 },
            { "2", 1 },
        };
        var sortedArray = sortedList.ToArray();
        Assert.Equal(0, sortedArray[0].Value);
        Assert.Equal(1, sortedArray[1].Value);
        Assert.Equal(2, sortedArray[2].Value);
    }
}
