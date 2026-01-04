namespace LTS.Common
{
    public record Result<T>(bool Success, T? Value, string? ErrorMessage)
    {
        public static Result<T> Ok(T value) => new(true, value, null);
        public static Result<T> Fail(string errorMessage) => new(false, default, errorMessage);
    }
}
