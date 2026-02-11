using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LexoRank.Core;

namespace LexoRank.Benchmark;

public class MyBenchmark
{
    private readonly LexoRankManager _lexoRankManager = new(CommonCharacterSets.Base62);

    [Benchmark]
    public void Normal()
    {
        var sortedValue0 = "nopSDGpa51g6f1asd3g13SDFGf65g1a3dgfE345FGYMNPM0mlm";
        var sortedValue1 = "56145fSFD6A6D161ASD41fa6df19ADF61fa66asdf44tas233";
        _ = _lexoRankManager.Between(sortedValue0, sortedValue1);
    }
}

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<MyBenchmark>();
    }
}
