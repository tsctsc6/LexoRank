using LexoRank.Core;
using LexoRank.Core.Errors;
using RustSharp;

namespace LexoRank.Test;

public class InvalidLexoRankString
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(CommonCharacterSets.Digits);
        var result = lexoRankManager.Between("abc", string.Empty);
        switch (result)
        {
            case ErrResult<string, List<Error>>:
                Assert.True(true);
                break;
            default:
                Assert.Fail();
                break;
        }
    }
}
