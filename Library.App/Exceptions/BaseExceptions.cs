namespace Library.App.Exceptions;
/// <summary>Базовое исключение всего приложения.</summary>
public abstract class AppException : Exception
{
/// <summary>Короткий машиночитаемый код ошибки, например "not_found".</summary>
public string ErrorCode { get; }
protected AppException(string errorCode, string message, Exception? inner = null)
: base(message, inner)
{
ErrorCode = errorCode;
}
}
/// <summary>Нарушение бизнес-правила предметной области.</summary>
public abstract class DomainException : AppException
{
protected DomainException(string errorCode, string message)
: base(errorCode, message)
{
}
}