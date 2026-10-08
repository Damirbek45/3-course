namespace Library.App.Exceptions;
public sealed class NotFoundException : DomainException
{
public string EntityName { get; } // что не нашли: "Reader", "Book"
public object Key { get; } // по какому ключу искали
public NotFoundException(string entityName, object key)
: base("not_found", $"{entityName} с ключом '{key}' не найден.")
{
EntityName = entityName;
Key = key;
}
}