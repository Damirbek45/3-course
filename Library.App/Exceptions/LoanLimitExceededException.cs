namespace Library.App.Exceptions;

public sealed class LoanLimitExceededException(int readerId, int limit)
    : DomainException("loan_limit_exceeded", $"Читатель с ключом '{readerId}' достиг лимита выдач ({limit}).")
{
    public int ReaderId { get; } = readerId;
    public int Limit { get; } = limit;
}

