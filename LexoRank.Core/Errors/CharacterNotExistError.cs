namespace LexoRank.Core.Errors;

public record CharacterNotExistError(string Message) : Error(Message);
