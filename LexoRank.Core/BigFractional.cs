using System.Numerics;

namespace LexoRank.Core;

/// <summary>
/// Represents a fractional, which value is numerator / (denominatorBase * denominatorExponent)
/// Special design was made to accommodate LexoRank.
/// In LexoRank, treat strings as fractional.
/// The denominator of this fractional are always denominatorBase * denominatorExponent, denominatorBase is always same at the same application.
/// This fractional are only have two operations: plus and divide by 2.
/// </summary>
public readonly struct BigFractional
{
    /// <summary>
    /// Represents a fractional, which value is numerator / (denominatorBase * denominatorExponent)
    /// </summary>
    /// <param name="numerator"></param>
    /// <param name="denominatorBase"></param>
    /// <param name="denominatorExponent"></param>
    /// <exception cref="NotFiniteNumberException"></exception>
    public BigFractional(BigInteger numerator, int denominatorBase, int denominatorExponent)
    {
        if (denominatorBase == 0)
        {
            throw new NotFiniteNumberException();
        }
        if (denominatorBase < 0)
        {
            throw new ArgumentException("denominatorBase < 0");
        }
        Numerator = numerator;
        DenominatorBase = denominatorBase;
        DenominatorExponent = denominatorExponent;
    }

    public BigInteger Numerator { get; }
    public int DenominatorBase { get; }
    public int DenominatorExponent { get; }

    public static BigFractional operator +(BigFractional a, BigFractional b)
    {
        if (a.DenominatorBase != b.DenominatorBase)
        {
            throw new ArgumentException("a.DenominatorBase != b.DenominatorBase");
        }

        if (a.DenominatorExponent == b.DenominatorExponent)
        {
            return new BigFractional(
                a.Numerator + b.Numerator,
                a.DenominatorBase,
                b.DenominatorExponent
            );
        }

        if (a.DenominatorExponent < b.DenominatorExponent)
        {
            return b + a;
        }

        // a.DenominatorExponent > b.DenominatorExponent
        var multiplier = BigInteger.Pow(
            a.DenominatorBase,
            (int)(a.DenominatorExponent - b.DenominatorExponent)
        );
        var b2 = new BigFractional(
            b.Numerator * multiplier,
            a.DenominatorBase,
            a.DenominatorExponent
        );

        return new BigFractional(
            a.Numerator + b2.Numerator,
            a.DenominatorBase,
            a.DenominatorExponent
        );
    }

    public BigFractional DivideByTwo()
    {
        var numerator = Numerator * DenominatorBase / 2;

        // Eliminate trailing zeros
        var i = DenominatorExponent + 1;
        while (i > 0)
        {
            var (quotient, remainder) = BigInteger.DivRem(numerator, DenominatorBase);
            if (remainder != 0)
                break;
            numerator = quotient;
            i--;
        }

        return new BigFractional(numerator, DenominatorBase, i);
    }
}
