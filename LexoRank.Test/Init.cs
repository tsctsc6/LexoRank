using LexoRank.Core;

namespace LexoRank.Test;

public class Init
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(CommonCharacterSets.Digits);
        var sortedValue = lexoRankManager.Between(string.Empty, string.Empty).Unwrap();
        Assert.Equal("5", sortedValue);
    }
}
