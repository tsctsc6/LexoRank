using LexoRank.Core;

namespace LexoRank.Test;

public class Init
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager("0123456789");
        var sortedValue = lexoRankManager.Between(string.Empty, string.Empty).Unwrap();
        Assert.Equal("5", sortedValue);
    }
}
