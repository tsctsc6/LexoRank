using System.Collections.Frozen;
using System.Numerics;
using LexoRank.Core.Errors;
using RustSharp;

namespace LexoRank.Core;

public class LexoRankManager
{
    public FrozenDictionary<char, BigInteger> CharacterToBigIntegerMap { get; init; }
    public char[] BigIntegerToCharacterMap { get; init; }

    public int BaseNumber { get; init; } = 0;

    public LexoRankManager(string characterSet)
    {
        BigIntegerToCharacterMap = characterSet.ToCharArray();
        var characterSet2 = new Dictionary<char, BigInteger>();
        foreach (var (index, character) in BigIntegerToCharacterMap.Index())
        {
            characterSet2.Add(character, index);
        }
        CharacterToBigIntegerMap = characterSet2.ToFrozenDictionary();
        BaseNumber = characterSet2.Count;
    }

    public Result<string, List<Error>> Between(string prev, string next)
    {
        var prevBigFractionalResult = string.IsNullOrEmpty(prev)
            ? BigFractional.Create(0, BaseNumber, 0)
            : GetBigFractionalFromLexoRankString(prev);
        var nextBigFractionalResult = string.IsNullOrEmpty(next)
            ? BigFractional.Create(1, BaseNumber, 0)
            : GetBigFractionalFromLexoRankString(next);
        var prevBigFractional = BigFractional.Zero;
        switch (prevBigFractionalResult)
        {
            case ErrResult<BigFractional, List<Error>> errResult:
                errResult.Value.Add(
                    new LexoRankFormatError($"Can't parse the prev, which value is \"{prev}\"")
                );
                return Result.Err(errResult.Value);
                break;
            case OkResult<BigFractional, List<Error>> okResult:
                prevBigFractional = okResult.Value;
                break;
        }
        var nextBigFractional = BigFractional.One;
        switch (nextBigFractionalResult)
        {
            case ErrResult<BigFractional, List<Error>> errResult:
                errResult.Value.Add(
                    new LexoRankFormatError($"Can't parse the next, which value is \"{next}\"")
                );
                return Result.Err(errResult.Value);
                break;
            case OkResult<BigFractional, List<Error>> okResult:
                nextBigFractional = okResult.Value;
                break;
        }
        var meanBigFractionalResult = BigFractional.Average(prevBigFractional, nextBigFractional);
        var meanBigFractional = BigFractional.Zero;
        switch (meanBigFractionalResult)
        {
            case ErrResult<BigFractional, List<Error>> errResult:
                errResult.Value.Add(new CalculateLexoRankError(string.Empty));
                return Result.Err(errResult.Value);
                break;
            case OkResult<BigFractional, List<Error>> okResult:
                meanBigFractional = okResult.Value;
                break;
        }
        return Result.Ok(GetLexoRankStringFromBigFractional(meanBigFractional));
    }

    private Result<BigFractional, List<Error>> GetBigFractionalFromLexoRankString(string str)
    {
        var numerator = BigInteger.Zero;
        var baseTimesExponent = BigInteger.One;
        var charArray = str.ToCharArray();
        Array.Reverse(charArray);
        var baseNumberBigInt = new BigInteger(BaseNumber);
        foreach (var character in charArray)
        {
            if (!CharacterToBigIntegerMap.TryGetValue(character, out var charValue))
            {
                return Result.Err(
                    new List<Error>([
                        new CharacterNotExistError(
                            $"Character '{character}' does not exist in CharacterSet."
                        ),
                    ])
                );
            }
            numerator += charValue * baseTimesExponent;
            baseTimesExponent *= baseNumberBigInt;
        }
        return BigFractional.Create(numerator, BaseNumber, str.Length);
    }

    private string GetLexoRankStringFromBigFractional(BigFractional bigFractional)
    {
        var charArray = new char[bigFractional.DenominatorExponent];
        var baseNumberBigInt = new BigInteger(BaseNumber);
        var numerator = bigFractional.Numerator;
        for (var i = charArray.Length - 1; i >= 0; i--)
        {
            var (quotient, remainder) = BigInteger.DivRem(numerator, baseNumberBigInt);
            numerator = quotient;
            // remainder always less than BaseNumber
            charArray[i] = BigIntegerToCharacterMap[(ulong)remainder];
        }
        return new string(charArray);
    }
}
