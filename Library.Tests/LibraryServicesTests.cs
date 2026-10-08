using Library.App.Data;
using Library.App.Exceptions;
using Library.App.Services;

namespace Library.Tests;

public class LibraryServicesTests
{
    private readonly InMemoryLibraryRepository repo = new();

    [Fact]
    public void RegisterReaderAddsReaderAndReturnsOk()
    {
        var count = repo.GetReaders().Count;
        var result = new LibraryService(repo).RegisterReader("  Виктор  ", "  victor@example.com  ");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(count + 1, repo.GetReaders().Count);
        var reader = repo.FindReader(count + 1);
        Assert.NotNull(reader);
        Assert.Equal("Виктор", reader.Name);
        Assert.Equal("victor@example.com", reader.Email);
        Assert.False(reader.IsBlocked);
        Assert.Empty(reader.Loans);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void EmptyNameReturnsFailureWithoutSaving(string? name)
    {
        var count = repo.GetReaders().Count;
        repo.SimulateStorageFailure = true;

        var result = new LibraryService(repo).RegisterReader(name, "victor@example.com");

        Assert.False(result.IsSuccess);
        Assert.Equal("Имя читателя не должно быть пустым.", result.Error);
        Assert.Equal(count, repo.GetReaders().Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("victor")]
    [InlineData("victor.example.com")]
    public void EmailWithoutAtReturnsFailureWithoutSaving(string? email)
    {
        var count = repo.GetReaders().Count;
        repo.SimulateStorageFailure = true;

        var result = new LibraryService(repo).RegisterReader("Виктор", email);

        Assert.False(result.IsSuccess);
        Assert.Equal("Почта должна содержать символ @.", result.Error);
        Assert.Equal(count, repo.GetReaders().Count);
    }

    [Fact]
    public void RegistrationsUseCurrentReaderCountPlusOne()
    {
        var service = new LibraryService(repo);
        var count = repo.GetReaders().Count;

        Assert.True(service.RegisterReader("Виктор", "victor@example.com").IsSuccess);
        Assert.True(service.RegisterReader("Галина", "galina@example.com").IsSuccess);

        Assert.Equal("Виктор", repo.FindReader(count + 1)!.Name);
        Assert.Equal("Галина", repo.FindReader(count + 2)!.Name);
        Assert.Equal(count + 2, repo.GetReaders().Count);
    }

    [Fact]
    public void FailedSaveRollsBackRegistrationAndAllowsRetry()
    {
        var service = new LibraryService(repo);
        var count = repo.GetReaders().Count;
        repo.SimulateStorageFailure = true;

        var error = Assert.Throws<DataAccessException>(() =>
            service.RegisterReader("Виктор", "victor@example.com"));

        Assert.IsType<IOException>(error.InnerException);
        Assert.Equal(count, repo.GetReaders().Count);
        Assert.Null(repo.FindReader(count + 1));

        repo.SimulateStorageFailure = false;
        Assert.True(service.RegisterReader("Виктор", "victor@example.com").IsSuccess);
        Assert.Equal(count + 1, repo.GetReaders().Count);
        Assert.Equal("Виктор", repo.FindReader(count + 1)!.Name);
    }
}
