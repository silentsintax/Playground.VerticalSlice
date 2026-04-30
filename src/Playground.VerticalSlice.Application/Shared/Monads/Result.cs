namespace Playground.VerticalSlice.Application.Shared.Monads
{
    public readonly struct Result<T>
    {
        private readonly T? _value;
        private readonly Error? _error;

        public bool IsSuccess { get; }
        public bool isFailure => !IsSuccess;

        public T Value => IsSuccess ? _value!
            : throw new InvalidOperationException($"Cannot access the value of a failed result. Error: {_error}");

        public Error Error => isFailure ? _error!
            : throw new InvalidOperationException("Cannot access the error of a successful result.");

        private Result(T value) { _value = value; IsSuccess = true; _error = default; }
        private Result(Error error) { _error = error; IsSuccess = false; _value = default; }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);


        //Monadic operations
        public Result<TOut> Map<TOut>(Func<T, TOut> map)
            => IsSuccess 
                ? Result<TOut>.Success(map(_value!)) 
                : Result<TOut>.Failure(_error!);

        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
            => IsSuccess
                ? bind(_value!)
                : Result<TOut>.Failure(_error!);

        public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> bind)
            => IsSuccess
                ? await bind(_value!)
                : Result<TOut>.Failure(_error!);

        public Result<T> Tap(Action<T> action) { 
            if (IsSuccess) action(_value!);
            return this;
        }

        public Result<T> TapError(Action<Error> action)
        {
            if (isFailure) action(_error!);
            return this;
        }

        public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
            => IsSuccess ? onSuccess(_value!) : onFailure(_error!);

        public Result<T> Recover(Func<Error, T> recover)
            => isFailure ? Result<T>.Success(recover(_error!)) : this;

        public Result<T> RecoverWhen(ErrorType errorType, Func<Error, T> recover)
            => isFailure && _error!.ErrorType == errorType ? Result<T>.Success(recover(_error)) : this;

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);

        public override string ToString() => IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
    }

    public static class Result
    {
        public static Result<T> Of<T>(Func<T> func)
        {
            try { return Result<T>.Success(func()); }
            catch (Exception ex) { return Result<T>.Failure(Error.InternalServerError(ex.Message, ex)); }
        }

        public static async Task<Result<T>> OfAsync<T>(Func<Task<T>> func)
        {
            try { return Result<T>.Success(await func()); }
            catch (Exception ex) { return Result<T>.Failure(Error.InternalServerError(ex.Message, ex)); }
        }
    }
}
