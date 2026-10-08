using Library.App.Models;
using Library.App.Exceptions;
namespace Library.App.Data;
public sealed class InMemoryLibraryRepository
{
private readonly List<Book> _books =
[
new() { Id = 1, Title = "CLR via C#", Author = "Jeffrey Richter" },
new() { Id = 2, Title = "C# in Depth", Author = "Jon Skeet" },
new() { Id = 3, Title = "Clean Code", Author = "Robert C. Martin" },
new() { Id = 4, Title = "Clean Architecture", Author = "Robert C. Martin" },
];
private readonly List<Reader> _readers =
[
new() { Id = 1, Name = "Анна", Email = "anna@example.com" },
new() { Id = 2, Name = "Борис", Email = "boris@example.com", IsBlocked = true },
];
///Если true — сохранение завершается ошибкой (нужно на уровне 2).
public bool SimulateStorageFailure { get; set; }
public Book? FindBook(int id) => _books.FirstOrDefault(b => b.Id == id);
public Reader? FindReader(int id) => _readers.FirstOrDefault(r => r.Id == id);
public List<Book> GetBooks() => _books;
public List<Reader> GetReaders() => _readers;
public void AddReader(Reader reader) => _readers.Add(reader);
public void Save()
{
    try
    {
        WriteToStorage();
    }
    catch (IOException ex)
    {
        throw new DataAccessException("Не удалось сохранить данные библиотеки.", ex);
    }
}
private void WriteToStorage()
{
if (SimulateStorageFailure)
throw new IOException("Диск недоступен: не удалось записать library.db");
// В реальном приложении здесь была бы запись в файл или БД
}
}
