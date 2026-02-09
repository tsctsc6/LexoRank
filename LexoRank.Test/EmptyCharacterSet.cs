using LexoRank.Core;
using LexoRank.Core.Errors;
using RustSharp;

namespace LexoRank.Test;

public class EmptyCharacterSet
{
    [Fact]
    public void Normal()
    {
        var lexoRankManager = new LexoRankManager(string.Empty);
        var result = lexoRankManager.Between(string.Empty, string.Empty);
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
