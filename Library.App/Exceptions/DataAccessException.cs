namespace Library.App.Exceptions;

public sealed class DataAccessException(string message, Exception? inner = null)
    : AppException("data_access_failed", message, inner);
