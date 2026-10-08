using Library.App.Data;
using Library.App.Exceptions;
using Library.App.Services;

namespace Library.App;

public static class Program
{
    public static void Main()
    {
        var repo = new InMemoryLibraryRepository();
        var service = new LibraryService(repo);

        Run("1. Анна берёт книгу 1", () => service.LoanBook(1, 1));
        Run("2. Анна снова берёт книгу 1", () => service.LoanBook(1, 1));
        Run("3. Несуществующий читатель 99", () => service.LoanBook(99, 2));
        Run("4. Заблокированный Борис берёт книгу 2", () => service.LoanBook(2, 2));
        Run("5. Анна берёт книги 2 и 3", () =>
        {
            service.LoanBook(1, 2);
            service.LoanBook(1, 3);
        });
        Run("6. Анна берёт четвёртую книгу", () => service.LoanBook(1, 4));
        Run("7. Анна берёт несуществующую книгу 99", () => service.LoanBook(1, 99));
        Run("8. Анна возвращает книгу 1", () => service.ReturnBook(1, 1));
        Run("9. Анна берёт книгу 4 после возврата", () => service.LoanBook(1, 4));
        Run("10. Анна повторно возвращает книгу 1", () => service.ReturnBook(1, 1));
        Run("11. Поиск книги CLR via C#", () =>
        {
            var book = service.FindByTitle("CLR via C#");
            Console.WriteLine(book is null ? "Книга не найдена" : $"Найдена книга: {book.Title}");
        });
        Run("12. Поиск несуществующей книги", () =>
        {
            var book = service.FindByTitle("Несуществующая книга");
            Console.WriteLine(book is null ? "Книга не найдена" : $"Найдена книга: {book.Title}");
        });
        Run("13. Ошибка сохранения при возврате книги 2", () =>
        {
            repo.SimulateStorageFailure = true;
            try
            {
                service.ReturnBook(1, 2);
            }
            finally
            {
                repo.SimulateStorageFailure = false;
            }
        });
    }

    private static void Run(string title, Action action)
    {
        Console.WriteLine($"--- {title}");
        try
        {
            action();
            Console.WriteLine("OK");
        }
        catch (ReaderBlockedException ex)
        {
            Console.WriteLine($"Читатель {ex.ReaderId} заблокирован. Обратитесь к библиотекарю");
        }
        catch (BookNotFoundException ex)
        {
            Console.WriteLine($"Книга {ex.BookId} не найдена. Проверьте номер книги");
        }
        catch (BookAlreadyLoanedException)
        {
            Console.WriteLine("Выберите другую книгу");
        }
        catch (LoanLimitExceededException)
        {
            Console.WriteLine("Сначала верните одну из книг");
        }
        catch (NotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (LoanNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (DataAccessException ex)
        {
            Console.WriteLine($"Ошибка сохранения: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
        }
    }
}

