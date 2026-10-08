namespace Library.App.Exceptions;

public sealed class ReaderBlockedException(int readerId)
    : DomainException("reader_blocked", $"Читатель с ключом '{readerId}' заблокирован.")
{
    public int ReaderId { get; } = readerId;
}

