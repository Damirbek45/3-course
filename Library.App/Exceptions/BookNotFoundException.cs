namespace Library.App.Exceptions;

public sealed class BookNotFoundException(int bookId)
    : DomainException("book_not_found", $"Книга с ключом '{bookId}' не найдена.")
{
    public int BookId { get; } = bookId;
}

