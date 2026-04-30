using System;
using System.Collections.Generic;
using System.Text;

namespace Playground.VeticalSlice.UnitTests.Application.Shared.Monads
{
    using Xunit;
    using AutoFixture;
    using Playground.VerticalSlice.Application.Shared.Monads;

    public class ResultTests
    {
        private readonly IFixture _fixture = new Fixture();

        #region Success Path

        [Fact]
        public void Success_WithValidValue_CreatesSuccessResult()
        {
            var value = _fixture.Create<string>();
            var result = Result<string>.Success(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.isFailure);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void Failure_WithError_CreatesFailureResult()
        {
            var error = Error.NotFound("Resource not found");
            var result = Result<string>.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.isFailure);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ImplicitOperator_WithValue_CreatesSuccessResult()
        {
            int value = _fixture.Create<int>();
            Result<int> result = value;

            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void ImplicitOperator_WithError_CreatesFailureResult()
        {
            var error = Error.Validation("Invalid input");
            Result<string> result = error;

            Assert.True(result.isFailure);
            Assert.Equal(error, result.Error);
        }

        #endregion

        #region Exception Handling

        [Fact]
        public void AccessValueOnFailure_ThrowsInvalidOperationException()
        {
            var result = Result<string>.Failure(Error.InternalServerError("Error occurred"));

            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void AccessErrorOnSuccess_ThrowsInvalidOperationException()
        {
            var result = Result<string>.Success("Success");

            Assert.Throws<InvalidOperationException>(() => result.Error);
        }

        #endregion

        #region Map Operations

        [Fact]
        public void Map_OnSuccess_AppliesFunctionAndReturnsSuccessResult()
        {
            var initialValue = 5;
            var result = Result<int>.Success(initialValue);

            var mappedResult = result.Map(x => x * 2);

            Assert.True(mappedResult.IsSuccess);
            Assert.Equal(10, mappedResult.Value);
        }

        [Fact]
        public void Map_OnFailure_ReturnsFailureWithoutApplyingFunction()
        {
            var error = Error.NotFound("Not found");
            var result = Result<int>.Failure(error);
            var mapCalled = false;

            var mappedResult = result.Map(x => { mapCalled = true; return x * 2; });

            Assert.False(mapCalled);
            Assert.True(mappedResult.isFailure);
            Assert.Equal(error, mappedResult.Error);
        }

        [Fact]
        public void Map_ChainMultipleMappings_AppliesAllTransformations()
        {
            var result = Result<int>.Success(5);

            var chainedResult = result
                .Map(x => x * 2)
                .Map(x => x + 3)
                .Map(x => x.ToString());

            Assert.True(chainedResult.IsSuccess);
            Assert.Equal("13", chainedResult.Value);
        }

        #endregion

        #region Bind Operations

        [Fact]
        public void Bind_OnSuccess_AppliesFunctionReturningResult()
        {
            var result = Result<int>.Success(5);

            var boundResult = result.Bind(x => Result<int>.Success(x * 2));

            Assert.True(boundResult.IsSuccess);
            Assert.Equal(10, boundResult.Value);
        }

        [Fact]
        public void Bind_OnSuccessReturningFailure_PropagatesFailure()
        {
            var error = Error.Validation("Invalid");
            var result = Result<int>.Success(5);

            var boundResult = result.Bind(x => Result<int>.Failure(error));

            Assert.True(boundResult.isFailure);
            Assert.Equal(error, boundResult.Error);
        }

        [Fact]
        public void Bind_OnFailure_ReturnsFailureWithoutApplyingFunction()
        {
            var error = Error.InternalServerError("Error");
            var result = Result<int>.Failure(error);

            var boundResult = result.Bind(x => Result<int>.Success(x * 2));

            Assert.True(boundResult.isFailure);
            Assert.Equal(error, boundResult.Error);
        }

        [Fact]
        public void Bind_ChainOperations_PropagatesFirstFailure()
        {
            var error1 = Error.NotFound("Not found");
            var error2 = Error.Validation("Invalid");

            var result = Result<int>.Success(5)
                .Bind(x => Result<int>.Failure(error1))
                .Bind(x => Result<int>.Failure(error2));

            Assert.True(result.isFailure);
            Assert.Equal(error1, result.Error);
        }

        #endregion

        #region BindAsync Operations

        [Fact]
        public async Task BindAsync_OnSuccess_AppliesAsyncFunction()
        {
            var result = Result<int>.Success(5);

            var boundResult = await result.BindAsync(x => Task.FromResult(Result<int>.Success(x * 2)));

            Assert.True(boundResult.IsSuccess);
            Assert.Equal(10, boundResult.Value);
        }

        [Fact]
        public async Task BindAsync_OnSuccessReturningFailure_PropagatesFailure()
        {
            var error = Error.Validation("Invalid");
            var result = Result<int>.Success(5);

            var boundResult = await result.BindAsync(x => Task.FromResult(Result<int>.Failure(error)));

            Assert.True(boundResult.isFailure);
            Assert.Equal(error, boundResult.Error);
        }

        [Fact]
        public async Task BindAsync_OnFailure_ReturnsFailureWithoutAwaitingFunction()
        {
            var error = Error.InternalServerError("Error");
            var result = Result<int>.Failure(error);
            var taskExecuted = false;

            var boundResult = await result.BindAsync(async x =>
            {
                taskExecuted = true;
                await Task.Delay(0);
                return Result<int>.Success(x);
            });

            Assert.False(taskExecuted);
            Assert.True(boundResult.isFailure);
        }

        #endregion

        #region Tap Operations

        [Fact]
        public void Tap_OnSuccess_ExecutesActionAndReturnsResult()
        {
            var result = Result<int>.Success(5);
            var executed = false;
            int capturedValue = 0;

            var tappedResult = result.Tap(x => { executed = true; capturedValue = x; });

            Assert.True(executed);
            Assert.Equal(5, capturedValue);
            Assert.True(tappedResult.IsSuccess);
            Assert.Equal(5, tappedResult.Value);
        }

        [Fact]
        public void Tap_OnFailure_SkipsActionAndReturnsResult()
        {
            var result = Result<int>.Failure(Error.NotFound("Not found"));
            var executed = false;

            var tappedResult = result.Tap(x => executed = true);

            Assert.False(executed);
            Assert.True(tappedResult.isFailure);
        }

        [Fact]
        public void TapError_OnFailure_ExecutesActionAndReturnsResult()
        {
            var error = Error.Validation("Invalid");
            var result = Result<int>.Failure(error);
            var executed = false;
            Error? capturedError = null;

            var tappedResult = result.TapError(err => { executed = true; capturedError = err; });

            Assert.True(executed);
            Assert.Equal(error, capturedError);
            Assert.True(tappedResult.isFailure);
        }

        [Fact]
        public void TapError_OnSuccess_SkipsActionAndReturnsResult()
        {
            var result = Result<int>.Success(5);
            var executed = false;

            var tappedResult = result.TapError(err => executed = true);

            Assert.False(executed);
            Assert.True(tappedResult.IsSuccess);
        }

        #endregion

        #region Match Operations

        [Fact]
        public void Match_OnSuccess_ExecutesSuccessFunc()
        {
            var result = Result<int>.Success(10);

            var matched = result.Match(
                onSuccess: x => x * 2,
                onFailure: err => 0
            );

            Assert.Equal(20, matched);
        }

        [Fact]
        public void Match_OnFailure_ExecutesFailureFunc()
        {
            var error = Error.NotFound("Not found");
            var result = Result<int>.Failure(error);

            var matched = result.Match(
                onSuccess: x => x * 2,
                onFailure: err => -1
            );

            Assert.Equal(-1, matched);
        }

        [Fact]
        public void Match_AccessesErrorDetails()
        {
            var error = Error.Validation("Validation failed");
            var result = Result<int>.Failure(error);

            var errorMessage = result.Match(
                onSuccess: x => "Success",
                onFailure: err => err.Message
            );

            Assert.Equal("Validation failed", errorMessage);
        }

        #endregion

        #region Recover Operations

        [Fact]
        public void Recover_OnFailure_RecoverValueAndReturnsSuccess()
        {
            var result = Result<int>.Failure(Error.InternalServerError("Error"));

            var recovered = result.Recover(err => 42);

            Assert.True(recovered.IsSuccess);
            Assert.Equal(42, recovered.Value);
        }

        [Fact]
        public void Recover_OnSuccess_ReturnsResultUnchanged()
        {
            var result = Result<int>.Success(10);

            var recovered = result.Recover(err => 42);

            Assert.True(recovered.IsSuccess);
            Assert.Equal(10, recovered.Value);
        }

        [Fact]
        public void RecoverWhen_OnFailureWithMatchingErrorType_RecoverValueAndReturnsSuccess()
        {
            var error = Error.NotFound("Not found");
            var result = Result<int>.Failure(error);

            var recovered = result.RecoverWhen(ErrorType.NotFound, err => 99);

            Assert.True(recovered.IsSuccess);
            Assert.Equal(99, recovered.Value);
        }

        [Fact]
        public void RecoverWhen_OnFailureWithNonMatchingErrorType_ReturnsFailureUnchanged()
        {
            var error = Error.NotFound("Not found");
            var result = Result<int>.Failure(error);

            var recovered = result.RecoverWhen(ErrorType.Validation, err => 99);

            Assert.True(recovered.isFailure);
            Assert.Equal(error, recovered.Error);
        }

        [Fact]
        public void RecoverWhen_OnSuccess_ReturnsResultUnchanged()
        {
            var result = Result<int>.Success(10);

            var recovered = result.RecoverWhen(ErrorType.NotFound, err => 42);

            Assert.True(recovered.IsSuccess);
            Assert.Equal(10, recovered.Value);
        }

        #endregion

        #region Of Factory Methods

        [Fact]
        public void Of_WithSuccessfulFunction_ReturnsSuccessResult()
        {
            var result = Result.Of(() => 42);

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Of_WithExceptionThrowingFunction_ReturnsFailureWithInternalServerError()
        {
            var exceptionMessage = "Something went wrong";
            var result = Result.Of<int>(() => throw new InvalidOperationException(exceptionMessage));

            Assert.True(result.isFailure);
            Assert.Equal(ErrorType.InternalServerError, result.Error.ErrorType);
            Assert.Contains(exceptionMessage, result.Error.Message);
            Assert.NotNull(result.Error.Exception);
        }

        [Fact]
        public async Task OfAsync_WithSuccessfulFunction_ReturnsSuccessResult()
        {
            var result = await Result.OfAsync(() => Task.FromResult(42));

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public async Task OfAsync_WithExceptionThrowingFunction_ReturnsFailureWithInternalServerError()
        {
            var exceptionMessage = "Async error occurred";
            var result = await Result.OfAsync<int>(() => throw new InvalidOperationException(exceptionMessage));

            Assert.True(result.isFailure);
            Assert.Equal(ErrorType.InternalServerError, result.Error.ErrorType);
            Assert.Contains(exceptionMessage, result.Error.Message);
        }

        #endregion

        #region ToString

        [Fact]
        public void ToString_OnSuccess_ReturnsSuccessMessage()
        {
            var result = Result<int>.Success(42);
            var str = result.ToString();

            Assert.Contains("Success", str);
            Assert.Contains("42", str);
        }

        [Fact]
        public void ToString_OnFailure_ReturnsFailureMessage()
        {
            var error = Error.Validation("Invalid input");
            var result = Result<string>.Failure(error);
            var str = result.ToString();

            Assert.Contains("Failure", str);
            Assert.Contains("Invalid input", str);
        }

        #endregion
    }
}
