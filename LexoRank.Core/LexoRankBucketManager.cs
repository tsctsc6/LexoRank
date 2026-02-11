using System.Numerics;

namespace LexoRank.Core;

public class LexoRankBucketManager
{
    public LexoRankManager LexoRankManager { get; init; }

    public char Separator { get; init; }

    public string[] Buckets { get; init; }

    public string CurrentBucket { get; set; } = string.Empty;

    public string NextBucket { get; set; } = string.Empty;

    private bool _isDesc = true;

    private BigFractional _stepSize = BigFractional.One;

    private BigFractional _lastLexoRankValue = BigFractional.One;

    private BigFractional _nextLexoRankValue = BigFractional.One;

    public LexoRankBucketManager(ReadOnlySpan<char> characterSet, char separator, string[] buckets)
    {
        LexoRankManager = new(characterSet);
        Separator = separator;
        Buckets = buckets;
    }

    public string Between(string prev, string next)
    {
        var prevBucketAndRank = prev.Split(Separator);
        var nextBucketAndRank = next.Split(Separator);
        if (prevBucketAndRank.Length != 2 || nextBucketAndRank.Length != 2)
        {
            throw new ArgumentException(
                $"Invalid value {nameof(prev)} or {nameof(next)}: {prev}, {next}"
            );
        }

        if (prevBucketAndRank[0] != nextBucketAndRank[0])
        {
            throw new ArgumentException(
                $"Invalid value {nameof(prev)} or {nameof(next)}: {prev}, {next}"
            );
        }
        var rank = LexoRankManager.Between(prevBucketAndRank[1], nextBucketAndRank[1]);
        return prevBucketAndRank[0] + Separator + rank;
    }

    public bool Begin(BigInteger count, double averageProportion)
    {
        var index = Buckets.IndexOf(CurrentBucket);
        if (index == -1)
            return false;
        _isDesc = index != Buckets.Length - 1;
        NextBucket = Buckets[(index + 1) % Buckets.Length];

        var averageProportionInt = (int)(LexoRankManager.BaseNumber * averageProportion);
        var exponent = 0;
        checked
        {
            exponent = (int)(BigInteger.Log(count, LexoRankManager.BaseNumber) + 1);
        }

        var step =
            BigInteger.Pow(LexoRankManager.BaseNumber, exponent) * averageProportionInt / count;
        _stepSize = _isDesc
            ? BigFractional.Create(BigInteger.Zero - step, LexoRankManager.BaseNumber, exponent + 1)
            : BigFractional.Create(step, LexoRankManager.BaseNumber, exponent + 1);
        _lastLexoRankValue = _isDesc
            ? BigFractional.Create(1, LexoRankManager.BaseNumber, exponent + 1)
            : BigFractional.Create(0, LexoRankManager.BaseNumber, exponent + 1);

        return true;
    }

    public string GetNewLexoRank()
    {
        return _isDesc ? GetNewLexoRankDesc() : GetNewLexoRankAsc();
    }

    private string GetNewLexoRankDesc()
    {
        var value = _lastLexoRankValue + _stepSize;
        string rank;
        if (BigInteger.IsPositive(value.Numerator))
        {
            rank = LexoRankManager.GetLexoRankStringFromBigFractional(value);
        }
        else
        {
            value = BigFractional.Average(
                BigFractional.Create(
                    0,
                    _lastLexoRankValue.DenominatorBase,
                    _lastLexoRankValue.DenominatorExponent
                ),
                _lastLexoRankValue
            );
            rank = LexoRankManager.GetLexoRankStringFromBigFractional(value);
        }
        _nextLexoRankValue = value;
        return NextBucket + Separator + rank;
    }

    private string GetNewLexoRankAsc()
    {
        var value = _lastLexoRankValue + _stepSize;
        string rank;
        if (!value.IsGreaterThanOrEqualToOne())
        {
            rank = LexoRankManager.GetLexoRankStringFromBigFractional(value);
        }
        else
        {
            value = BigFractional.Average(
                BigFractional.Create(
                    1,
                    _lastLexoRankValue.DenominatorBase,
                    _lastLexoRankValue.DenominatorExponent
                ),
                _lastLexoRankValue
            );
            rank = LexoRankManager.GetLexoRankStringFromBigFractional(value);
        }
        _nextLexoRankValue = value;
        return NextBucket + Separator + rank;
    }

    public void Next()
    {
        _lastLexoRankValue = _nextLexoRankValue;
    }

    public LexoRankBucketManagerState GetState()
    {
        return new()
        {
            CharacterSet = new string(LexoRankManager.IntToCharacterMap),
            Separator = Separator,
            Buckets = Buckets,
            CurrentBucket = CurrentBucket,
            NextBucket = NextBucket,
            IsDesc = _isDesc,
            DenominatorBase = _stepSize.DenominatorBase,
            DenominatorExponent = _stepSize.DenominatorExponent,
            StepSizeNumerator = _stepSize.Numerator.ToString(),
            LastLexoRankValueNumerator = _lastLexoRankValue.Numerator.ToString(),
        };
    }

    public static LexoRankBucketManager Create(LexoRankBucketManagerState state)
    {
        var obj = new LexoRankBucketManager(state.CharacterSet, state.Separator, state.Buckets);
        obj.CurrentBucket = state.CurrentBucket;
        obj.NextBucket = state.NextBucket;
        obj._isDesc = state.IsDesc;
        obj._stepSize = BigFractional.Create(
            BigInteger.Parse(state.StepSizeNumerator),
            state.DenominatorBase,
            state.DenominatorExponent
        );
        obj._lastLexoRankValue = BigFractional.Create(
            BigInteger.Parse(state.LastLexoRankValueNumerator),
            state.DenominatorBase,
            state.DenominatorExponent
        );
        return obj;
    }
}
