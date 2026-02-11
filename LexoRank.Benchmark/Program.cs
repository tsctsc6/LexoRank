using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LexoRank.Core;

namespace LexoRank.Benchmark;

public class MyBenchmark
{
    private readonly LexoRankManager _lexoRankManager = new(CommonCharacterSets.Base62);
    private readonly string _sortedValue0 = Random.Shared.GetString(
        CommonCharacterSets.Base62,
        100
    );
    private readonly string _sortedValue1 = Random.Shared.GetString(
        CommonCharacterSets.Base62,
        100
    );

    [Benchmark]
    public void Normal()
    {
        _ = _lexoRankManager.Between(_sortedValue0, _sortedValue1);
    }
}

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<MyBenchmark>();
    }
}
