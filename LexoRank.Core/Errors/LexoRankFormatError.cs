namespace LexoRank.Core.Errors;

public record LexoRankFormatError(string Message) : Error(Message);
