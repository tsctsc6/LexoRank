namespace LexoRank.Core.Errors;

public record NotFiniteNumberError(string Message) : Error(Message);
