namespace LexoRank.Core.Errors;

public record CalculateLexoRankError(string Message) : Error(Message);
