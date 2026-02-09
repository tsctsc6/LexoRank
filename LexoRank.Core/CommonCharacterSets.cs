namespace LexoRank.Core;

public static class CommonCharacterSets
{
    public const string Digits = "0123456789";
    public const string UpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string LowerLetters = "abcdefghijklmnopqrstuvwxyz";
    public const string Base36 = Digits + UpperLetters;
    public const string Base62 = Digits + UpperLetters + LowerLetters;
}
