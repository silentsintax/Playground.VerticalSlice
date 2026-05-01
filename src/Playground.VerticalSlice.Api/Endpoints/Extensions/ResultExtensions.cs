using Microsoft.AspNetCore.Http.HttpResults;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VerticalSlice.Api.Endpoints.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToHttpResult<T>(
            this Result<T> result,
            Func<T, IResult> onSuccess)
            => result.Match(
                onSuccess,
                error => error.ToProblem());

        public static IResult ToOk<T>(this Result<T> result)
            => result.ToHttpResult(value => TypedResults.Ok(value));

        public static async Task<IResult> ToOk<T>(this Task<Result<T>> resultTask)
            => (await resultTask).ToOk();

        public static IResult ToCreated<T>(
            this Result<T> result,
            Func<T, string> locationBuilder)
            => result.ToHttpResult(value =>
                TypedResults.Created(locationBuilder(value), value));

        public static async Task<IResult> ToCreated<T>(
            this Task<Result<T>> resultTask,
            Func<T, string> locationBuilder)
            => (await resultTask).ToCreated(locationBuilder);

        public static IResult ToCreated<T, TResponse>(
            this Result<T> result,
            Func<T, string> locationBuilder,
            Func<T, TResponse> mapper)
            => result.ToHttpResult(value =>
                TypedResults.Created(locationBuilder(value), mapper(value)));

        public static async Task<IResult> ToCreated<T, TResponse>(
            this Task<Result<T>> resultTask,
            Func<T, string> locationBuilder,
            Func<T, TResponse> mapper)
            => (await resultTask).ToCreated(locationBuilder, mapper);

        public static IResult ToNoContent<T>(this Result<T> result)
            => result.ToHttpResult(_ => TypedResults.NoContent());

        public static async Task<IResult> ToNoContent<T>(this Task<Result<T>> resultTask)
            => (await resultTask).ToNoContent();

        public static Ok<T> ToTypedOk<T>(this Result<T> result)
            where T : class
            => result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : throw new InvalidOperationException("Result is a failure.");

        public static Created<T> ToTypedCreated<T>(
            this Result<T> result,
            Func<T, string> locationBuilder)
            where T : class
            => result.IsSuccess
                ? TypedResults.Created(locationBuilder(result.Value), result.Value)
                : throw new InvalidOperationException("Result is a failure.");

        public static Created<TResponse> ToTypedCreated<T, TResponse>(
            this Result<T> result,
            Func<T, string> locationBuilder,
            Func<T, TResponse> mapper)
            where TResponse : class
            => result.IsSuccess
                ? TypedResults.Created(locationBuilder(result.Value), mapper(result.Value))
                : throw new InvalidOperationException("Result is a failure.");

        public static IResult ToHttpResult<T>(
            this Result<T> result,
            Func<T, IResult> onSuccess,
            Func<Error, IResult> onFailure)
            => result.Match(onSuccess, onFailure);

        public static async Task<IResult> ToHttpResult<T>(
            this Task<Result<T>> resultTask,
            Func<T, IResult> onSuccess,
            Func<Error, IResult> onFailure)
        {
            var result = await resultTask;
            return result.ToHttpResult(onSuccess, onFailure);
        }

        public static IResult ToProblem(this Error error)
        {
            var (status, title) = error.ErrorType switch
            {
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource Not Found"),
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                ErrorType.InternalServerError => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
                _ => (StatusCodes.Status500InternalServerError, "Unknown Error")
            };

            var extensions = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["errorType"] = error.ErrorType.ToString(),
                ["errors"] = error.Messages
            };

            if (error.Exception is not null)
                extensions["exceptionMessage"] = error.Exception.Message;

            var detail = error.HasMultipleMessages
                ? $"{error.Messages.Count} errors occurred."
                : error.Message;

            return TypedResults.Problem(
                statusCode: status,
                title: title,
                detail: detail,
                extensions: extensions);
        }
    }
}
