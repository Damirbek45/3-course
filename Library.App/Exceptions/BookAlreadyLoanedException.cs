namespace Library.App.Exceptions;

public sealed class BookAlreadyLoanedException(int bookId)
    : DomainException("book_already_loaned", $"Книга с ключом '{bookId}' уже выдана.")
{
    public int BookId { get; } = bookId;
}

