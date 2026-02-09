using System.Numerics;
using LexoRank.Core.Errors;
using RustSharp;

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
    private BigFractional(BigInteger numerator, int denominatorBase, int denominatorExponent)
    {
        Numerator = numerator;
        DenominatorBase = denominatorBase;
        DenominatorExponent = denominatorExponent;
    }

    public static Result<BigFractional, List<Error>> Create(
        BigInteger numerator,
        int denominatorBase,
        int denominatorExponent
    )
    {
        return denominatorBase switch
        {
            <= 0 => Result.Err(new List<Error>([new NotFiniteNumberError(string.Empty)])),
            _ => Result.Ok(new BigFractional(numerator, denominatorBase, denominatorExponent)),
        };
    }

    public static BigFractional Zero => new BigFractional(0, 1, 0);
    public static BigFractional One => new BigFractional(1, 1, 0);

    public BigInteger Numerator { get; }
    public int DenominatorBase { get; }
    public int DenominatorExponent { get; }

    public static Result<BigFractional, List<Error>> TryAdd(BigFractional a, BigFractional b)
    {
        if (a.DenominatorBase != b.DenominatorBase)
        {
            return Result.Err(
                new List<Error>([
                    new DenominatorBaseDifferenceError(
                        $"a.DenominatorBase = {a.DenominatorBase}, b.DenominatorBase = {b.DenominatorBase}"
                    ),
                ])
            );
        }

        if (a.DenominatorExponent == b.DenominatorExponent)
        {
            return Result.Ok(
                new BigFractional(
                    a.Numerator + b.Numerator,
                    a.DenominatorBase,
                    b.DenominatorExponent
                )
            );
        }

        if (a.DenominatorExponent < b.DenominatorExponent)
        {
            return TryAdd(b, a);
        }

        // a.DenominatorExponent > b.DenominatorExponent
        var multiplier = BigInteger.Pow(
            a.DenominatorBase,
            a.DenominatorExponent - b.DenominatorExponent
        );
        var b2 = new BigFractional(
            b.Numerator * multiplier,
            a.DenominatorBase,
            a.DenominatorExponent
        );

        return Result.Ok(
            new BigFractional(a.Numerator + b2.Numerator, a.DenominatorBase, a.DenominatorExponent)
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

    public static Result<BigFractional, List<Error>> Average(BigFractional a, BigFractional b)
    {
        var sumResult = TryAdd(a, b);
        var sum = BigFractional.Zero;
        switch (sumResult)
        {
            case ErrResult<BigFractional, List<Error>> errResult:
                errResult.Value.Add(new CalculateAverageError(string.Empty));
                break;
            case OkResult<BigFractional, List<Error>> okResult:
                sum = okResult.Value;
                break;
        }
        return Result.Ok(sum.DivideByTwo());
    }
}
