namespace LexoRank.Core;

public class LexoRankBucketManagerState
{
    public string CharacterSet { get; set; }
    public char Separator { get; set; }
    public string[] Buckets { get; set; }
    public string CurrentBucket { get; set; }
    public string NextBucket { get; set; }
    public bool IsDesc { get; set; }
    public int DenominatorBase { get; set; }
    public int DenominatorExponent { get; set; }
    public string StepSizeNumerator { get; set; }
    public string LastLexoRankValueNumerator { get; set; }
    public bool IsRebalancing { get; set; }
}
