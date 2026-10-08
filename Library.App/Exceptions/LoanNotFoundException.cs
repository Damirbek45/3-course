namespace Library.App.Exceptions;

public sealed class LoanNotFoundException(int readerId, int bookId)
    : DomainException("loan_not_found",
        $"Активная выдача книги {bookId} читателю {readerId} не найдена.")
{
    public int ReaderId { get; } = readerId;
    public int BookId { get; } = bookId;
}
