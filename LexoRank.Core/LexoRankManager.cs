using System.Collections.Frozen;
using System.Numerics;

namespace LexoRank.Core;

public class LexoRankManager
{
    public FrozenDictionary<char, BigInteger> CharacterToBigIntegerMap { get; init; }
    public char[] BigIntegerToCharacterMap { get; init; }

    public int BaseNumber { get; init; } = 0;

    public LexoRankManager(ReadOnlySpan<char> characterSet)
    {
        BigIntegerToCharacterMap = characterSet.ToArray();
        var characterSet2 = new Dictionary<char, BigInteger>();
        foreach (var (index, character) in BigIntegerToCharacterMap.Index())
        {
            characterSet2.Add(character, index);
        }
        CharacterToBigIntegerMap = characterSet2.ToFrozenDictionary();
        BaseNumber = characterSet2.Count;
    }

    public string Between(string prev, string next)
    {
        var prevBigFractional = string.IsNullOrEmpty(prev)
            ? BigFractional.Create(0, BaseNumber, 0)
            : GetBigFractionalFromLexoRankString(prev);
        var nextBigFractional = string.IsNullOrEmpty(next)
            ? BigFractional.Create(1, BaseNumber, 0)
            : GetBigFractionalFromLexoRankString(next);
        var meanBigFractional = BigFractional.Average(prevBigFractional, nextBigFractional);
        return GetLexoRankStringFromBigFractional(meanBigFractional);
    }

    private BigFractional GetBigFractionalFromLexoRankString(string str)
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
                throw new ArgumentException(
                    $"Character '{character}' does not exist in CharacterSet."
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
