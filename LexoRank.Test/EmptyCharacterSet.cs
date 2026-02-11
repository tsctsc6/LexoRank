using LexoRank.Core;

namespace LexoRank.Test;

public class EmptyCharacterSet
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(string.Empty);
        try
        {
            _ = lexoRankManager.Between(string.Empty, string.Empty);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            Assert.True(true);
        }
    }
}
