using Library.App.Data;
using Library.App.Exceptions;
using Library.App.Models;

namespace Library.App.Services;

public class LibraryService(InMemoryLibraryRepository repo)
{
    public const int MaxActiveLoans = 3;

    public Result RegisterReader(string? name, string? email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Имя читателя не должно быть пустым.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result.Fail("Почта должна содержать символ @.");

        var reader = new Reader
        {
            Id = repo.GetReaders().Count + 1,
            Name = name.Trim(),
            Email = email.Trim()
        };
        repo.AddReader(reader);
        try
        {
            repo.Save();
        }
        catch
        {
            repo.GetReaders().Remove(reader);
            throw;
        }

        return Result.Ok();
    }

    public void LoanBook(int readerId, int bookId)
    {
        var reader = repo.FindReader(readerId)
            ?? throw new NotFoundException("Читатель", readerId);
        if (reader.IsBlocked)
            throw new ReaderBlockedException(readerId);

        var book = repo.FindBook(bookId)
            ?? throw new BookNotFoundException(bookId);
        if (book.Status == BookStatus.Loaned)
            throw new BookAlreadyLoanedException(bookId);
        if (reader.Loans.Count(l => !l.IsReturned) >= MaxActiveLoans)
            throw new LoanLimitExceededException(readerId, MaxActiveLoans);

        var loan = new Loan
        {
            BookId = bookId,
            DueDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14)
        };
        book.Status = BookStatus.Loaned;
        reader.Loans.Add(loan);
        try
        {
            repo.Save();
        }
        catch
        {
            book.Status = BookStatus.Available;
            reader.Loans.Remove(loan);
            throw;
        }
    }

    public void ReturnBook(int readerId, int bookId)
    {
        var reader = repo.FindReader(readerId)
            ?? throw new NotFoundException("Читатель", readerId);
        var book = repo.FindBook(bookId)
            ?? throw new NotFoundException("Книга", bookId);
        var loan = reader.Loans.FirstOrDefault(l => l.BookId == bookId && !l.IsReturned)
            ?? throw new LoanNotFoundException(readerId, bookId);

        var previousStatus = book.Status;
        loan.IsReturned = true;
        book.Status = BookStatus.Available;
        try
        {
            repo.Save();
        }
        catch
        {
            loan.IsReturned = false;
            book.Status = previousStatus;
            throw;
        }
    }

    public Book? FindByTitle(string title) =>
        repo.GetBooks().FirstOrDefault(b => b.Title == title);
}

