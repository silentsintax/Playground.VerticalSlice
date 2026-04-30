namespace Playground.VerticalSlice.Application.Shared.Monads
{
    public enum ErrorType
    {
        NotFound,
        Unauthorized,
        Conflict,
        InternalServerError,
        Validation
    }

    public sealed record Error
    {
        public ErrorType ErrorType { get; }
        public IReadOnlyList<string> Messages { get; set; }
        public Exception? Exception { get; }

        private Error(ErrorType errorType, string message, Exception? exception = null)
        {
            ErrorType = errorType;
            Messages = [message];
            Exception = exception;
        }

        private Error(ErrorType errorType, IEnumerable<string> messages, Exception? exception = null)
        {
            var errors = messages.ToList();
            if (!errors.Any())
                throw new ArgumentException("At least one error message must be provided.", nameof(messages));

            ErrorType = errorType;
            Exception = exception;
            Messages = errors.AsReadOnly();
        }

        //Por conveniencia, separo a primeira mensagem.
        public string Message => Messages[0];

        //Factory de erros especificos para cada tipo de erro, facilitando a criação de erros comuns.
        public static Error NotFound(string message)
            => new(ErrorType.NotFound, message);
        public static Error Unauthorized(string message)
           => new(ErrorType.Unauthorized, message);
        public static Error Validation(string message)
           => new(ErrorType.Validation, message);
        public static Error Conflict(string message)
           => new(ErrorType.Conflict, message);
        public static Error InternalServerError(string message, Exception? exception = null)
           => new(ErrorType.InternalServerError, message, exception);

        //Factory para erros em lista, útil para validação de múltiplos erros.

        public static Error NotFound(IEnumerable<string> messages)
          => new(ErrorType.NotFound, messages);
        public static Error Unauthorized(IEnumerable<string> messages)
           => new(ErrorType.Unauthorized, messages);
        public static Error Validation(IEnumerable<string> messages)
           => new(ErrorType.Validation, messages);
        public static Error Conflict(IEnumerable<string> messages)
           => new(ErrorType.Conflict, messages);
        public static Error InternalServerError(IEnumerable<string> messages, Exception? exception = null)
           => new(ErrorType.InternalServerError, messages, exception);

        //Helpers

        public bool HasMultipleMessages => Messages.Count > 1;

        public Error Append(string message)
            => new(ErrorType, Messages.Append(message), Exception);

        public Error Append(IEnumerable<string> messages)
            => new(ErrorType, Messages.Concat(messages), Exception);

        public override string ToString()
            => $"[{ErrorType}] {string.Join(" | ", Messages)}";
    }
}
