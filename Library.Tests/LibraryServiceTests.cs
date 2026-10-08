using Library.App.Data;
using Library.App.Exceptions;
using Library.App.Models;
using Library.App.Services;

namespace Library.Tests;

public class LibraryServiceTests
{
    private readonly InMemoryLibraryRepository repo = new();

    [Fact]
    public void LoanCreatesActiveLoanWithFourteenDayDueDate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        new LibraryService(repo).LoanBook(1, 1);
        Assert.Equal(BookStatus.Loaned, repo.FindBook(1)!.Status);
        var loan = Assert.Single(repo.FindReader(1)!.Loans);
        Assert.Equal(1, loan.BookId);
        Assert.False(loan.IsReturned);
        Assert.InRange(loan.DueDate, today.AddDays(14),
            DateOnly.FromDateTime(DateTime.Today).AddDays(14));
    }

    [Fact]
    public void UnknownReaderThrowsWithoutChangingBook()
    {
        var error = Assert.Throws<NotFoundException>(() => new LibraryService(repo).LoanBook(99, 1));
        Assert.Equal(99, error.Key);
        Assert.Equal(BookStatus.Available, repo.FindBook(1)!.Status);
    }

    [Fact]
    public void BlockedReaderCannotBorrow()
    {
        var error = Assert.Throws<ReaderBlockedException>(() => new LibraryService(repo).LoanBook(2, 1));
        Assert.Equal(2, error.ReaderId);
        Assert.Empty(repo.FindReader(2)!.Loans);
        Assert.Equal(BookStatus.Available, repo.FindBook(1)!.Status);
    }

    [Fact]
    public void MissingBookThrowsSpecificException()
    {
        var error = Assert.Throws<BookNotFoundException>(() => new LibraryService(repo).LoanBook(1, 99));
        Assert.Equal(99, error.BookId);
        Assert.Empty(repo.FindReader(1)!.Loans);
    }

    [Fact]
    public void DuplicateLoanDoesNotCreateAnotherRecord()
    {
        var service = new LibraryService(repo);
        service.LoanBook(1, 1);
        Assert.Throws<BookAlreadyLoanedException>(() => service.LoanBook(1, 1));
        Assert.Single(repo.FindReader(1)!.Loans);
    }

    [Fact]
    public void LoanLimitRejectsFourthBookAndReturnFreesCapacity()
    {
        var service = new LibraryService(repo);
        service.LoanBook(1, 1);
        service.LoanBook(1, 2);
        service.LoanBook(1, 3);
        var error = Assert.Throws<LoanLimitExceededException>(() => service.LoanBook(1, 4));
        Assert.Equal(3, error.Limit);
        Assert.Equal(BookStatus.Available, repo.FindBook(4)!.Status);
        Assert.Equal(3, repo.FindReader(1)!.Loans.Count);
        service.ReturnBook(1, 1);
        service.LoanBook(1, 4);
        Assert.Equal(BookStatus.Available, repo.FindBook(1)!.Status);
        Assert.Equal(3, repo.FindReader(1)!.Loans.Count(l => !l.IsReturned));
    }

    [Fact]
    public void ReturnedBookCanBeBorrowedAgain()
    {
        var service = new LibraryService(repo);
        service.LoanBook(1, 1);
        service.ReturnBook(1, 1);
        service.LoanBook(1, 1);
        Assert.Equal(2, repo.FindReader(1)!.Loans.Count);
        Assert.Single(repo.FindReader(1)!.Loans, l => !l.IsReturned);
    }

    [Theory]
    [InlineData(99, 1, typeof(NotFoundException))]
    [InlineData(1, 99, typeof(NotFoundException))]
    [InlineData(1, 1, typeof(LoanNotFoundException))]
    public void InvalidReturnReportsError(int readerId, int bookId, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => new LibraryService(repo).ReturnBook(readerId, bookId));
        Assert.Equal(BookStatus.Available, repo.FindBook(1)!.Status);
    }

    [Fact]
    public void ReaderCannotReturnSomeoneElsesLoan()
    {
        var service = new LibraryService(repo);
        service.LoanBook(1, 1);
        var error = Assert.Throws<LoanNotFoundException>(() => service.ReturnBook(2, 1));
        Assert.Equal(2, error.ReaderId);
        Assert.Equal(1, error.BookId);
        Assert.Equal(BookStatus.Loaned, repo.FindBook(1)!.Status);
        Assert.False(Assert.Single(repo.FindReader(1)!.Loans).IsReturned);
    }

    [Fact]
    public void FailedSaveRollsBackLoanAndPreservesStorageException()
    {
        repo.SimulateStorageFailure = true;
        var error = Assert.Throws<DataAccessException>(() => new LibraryService(repo).LoanBook(1, 1));
        Assert.Equal("data_access_failed", error.ErrorCode);
        var inner = Assert.IsType<IOException>(error.InnerException);
        Assert.Contains("WriteToStorage", inner.StackTrace);
        Assert.Contains("System.IO.IOException", error.ToString());
        Assert.Equal(BookStatus.Available, repo.FindBook(1)!.Status);
        Assert.Empty(repo.FindReader(1)!.Loans);
    }

    [Fact]
    public void FailedSaveRollsBackReturn()
    {
        var service = new LibraryService(repo);
        service.LoanBook(1, 1);
        repo.SimulateStorageFailure = true;
        var error = Assert.Throws<DataAccessException>(() => service.ReturnBook(1, 1));
        Assert.IsType<IOException>(error.InnerException);
        Assert.Equal(BookStatus.Loaned, repo.FindBook(1)!.Status);
        Assert.False(Assert.Single(repo.FindReader(1)!.Loans).IsReturned);
    }

    [Fact]
    public void SearchReturnsBookOrNull()
    {
        var service = new LibraryService(repo);
        Assert.Same(repo.FindBook(1), service.FindByTitle("CLR via C#"));
        Assert.Null(service.FindByTitle("Несуществующая книга"));
    }

    [Fact]
    public void RepeatedReturnReportsMissingActiveLoan()
    {
        var service = new LibraryService(repo);
        service.LoanBook(1, 1);
        service.ReturnBook(1, 1);
        var error = Assert.Throws<LoanNotFoundException>(() => service.ReturnBook(1, 1));
        Assert.Equal("loan_not_found", error.ErrorCode);
        Assert.True(Assert.Single(repo.FindReader(1)!.Loans).IsReturned);
        Assert.Equal(BookStatus.Available, repo.FindBook(1)!.Status);
    }

    [Fact]
    public void RepositoryWrapsStorageFailureAndPreservesItsStack()
    {
        repo.SimulateStorageFailure = true;
        var error = Assert.Throws<DataAccessException>(() => repo.Save());
        Assert.Equal("data_access_failed", error.ErrorCode);
        Assert.Equal("Не удалось сохранить данные библиотеки.", error.Message);
        var inner = Assert.IsType<IOException>(error.InnerException);
        Assert.Contains("WriteToStorage", inner.StackTrace);
        Assert.Contains("System.IO.IOException", error.ToString());
    }
}
