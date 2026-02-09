namespace LexoRank.Core.Errors;

public record DenominatorBaseDifferenceError(string Message) : Error(Message);
