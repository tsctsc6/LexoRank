using LexoRank.Core;

namespace LexoRank.Test;

public class InvalidLexoRankString
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(CommonCharacterSets.Digits);
        try
        {
            _ = lexoRankManager.Between("abc", string.Empty);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            Assert.True(true);
        }
    }
}
