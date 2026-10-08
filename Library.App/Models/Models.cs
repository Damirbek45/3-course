namespace Library.App.Models;
public enum BookStatus { Available, Loaned }
public sealed class Book
{
public required int Id { get; init; }
public required string Title { get; init; }
public required string Author { get; init; }
public BookStatus Status { get; set; } = BookStatus.Available;
}
public sealed class Reader
{
public required int Id { get; init; }
public required string Name { get; init; }
public required string Email { get; init; }
public bool IsBlocked { get; set; }
public List<Loan> Loans { get; } = [];
}
public sealed class Loan
{
public required int BookId { get; init; }
public required DateOnly DueDate { get; init; }
public bool IsReturned { get; set; }
}