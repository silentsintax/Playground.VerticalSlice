using Xunit;
using AutoFixture;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VeticalSlice.UnitTests.Application.Shared.Monads
{
    public class ErrorTests
    {
        private readonly IFixture _fixture = new Fixture();

        #region Factory Methods - Single Message

        [Fact]
        public void NotFound_WithMessage_CreatesErrorWithCorrectType()
        {
            var message = _fixture.Create<string>();
            var error = Error.NotFound(message);

            Assert.Equal(ErrorType.NotFound, error.ErrorType);
            Assert.Equal(message, error.Message);
            Assert.Single(error.Messages);
        }

        [Fact]
        public void Unauthorized_WithMessage_CreatesErrorWithCorrectType()
        {
            var message = _fixture.Create<string>();
            var error = Error.Unauthorized(message);

            Assert.Equal(ErrorType.Unauthorized, error.ErrorType);
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void Validation_WithMessage_CreatesErrorWithCorrectType()
        {
            var message = _fixture.Create<string>();
            var error = Error.Validation(message);

            Assert.Equal(ErrorType.Validation, error.ErrorType);
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void Conflict_WithMessage_CreatesErrorWithCorrectType()
        {
            var message = _fixture.Create<string>();
            var error = Error.Conflict(message);

            Assert.Equal(ErrorType.Conflict, error.ErrorType);
            Assert.Equal(message, error.Message);
        }

        [Fact]
        public void InternalServerError_WithMessage_CreatesErrorWithCorrectType()
        {
            var message = _fixture.Create<string>();
            var error = Error.InternalServerError(message);

            Assert.Equal(ErrorType.InternalServerError, error.ErrorType);
            Assert.Equal(message, error.Message);
            Assert.Null(error.Exception);
        }

        [Fact]
        public void InternalServerError_WithMessageAndException_StoresException()
        {
            var message = _fixture.Create<string>();
            var exception = new InvalidOperationException("Original error");
            var error = Error.InternalServerError(message, exception);

            Assert.Equal(ErrorType.InternalServerError, error.ErrorType);
            Assert.Equal(message, error.Message);
            Assert.Same(exception, error.Exception);
        }

        #endregion

        #region Factory Methods - Multiple Messages

        [Fact]
        public void NotFound_WithMultipleMessages_CreatesErrorWithAllMessages()
        {
            var messages = _fixture.CreateMany<string>(3).ToList();
            var error = Error.NotFound(messages);

            Assert.Equal(ErrorType.NotFound, error.ErrorType);
            Assert.Equal(messages.Count, error.Messages.Count);
            Assert.Equal(messages[0], error.Message);
            Assert.All(messages, msg => Assert.Contains(msg, error.Messages));
        }

        [Fact]
        public void Unauthorized_WithMultipleMessages_CreatesErrorWithAllMessages()
        {
            var messages = _fixture.CreateMany<string>(2).ToList();
            var error = Error.Unauthorized(messages);

            Assert.Equal(ErrorType.Unauthorized, error.ErrorType);
            Assert.Equal(messages.Count, error.Messages.Count);
        }

        [Fact]
        public void Validation_WithMultipleMessages_CreatesErrorWithAllMessages()
        {
            var messages = _fixture.CreateMany<string>(4).ToList();
            var error = Error.Validation(messages);

            Assert.Equal(ErrorType.Validation, error.ErrorType);
            Assert.Equal(4, error.Messages.Count);
        }

        [Fact]
        public void Conflict_WithMultipleMessages_CreatesErrorWithAllMessages()
        {
            var messages = new[] { "Conflict 1", "Conflict 2", "Conflict 3" };
            var error = Error.Conflict(messages);

            Assert.Equal(ErrorType.Conflict, error.ErrorType);
            Assert.Equal(3, error.Messages.Count);
        }

        [Fact]
        public void InternalServerError_WithMultipleMessagesAndException_CreatesCompleteError()
        {
            var messages = _fixture.CreateMany<string>(2).ToList();
            var exception = new Exception("Critical error");
            var error = Error.InternalServerError(messages, exception);

            Assert.Equal(ErrorType.InternalServerError, error.ErrorType);
            Assert.Equal(2, error.Messages.Count);
            Assert.Same(exception, error.Exception);
        }

        [Fact]
        public void FactoryWithMultipleMessages_WithEmptyCollection_ThrowsArgumentException()
        {
            var emptyMessages = Enumerable.Empty<string>();

            Assert.Throws<ArgumentException>(() => Error.NotFound(emptyMessages));
        }

        #endregion

        #region Message Properties

        [Fact]
        public void Message_ReturnFirstMessage()
        {
            var messages = new[] { "First", "Second", "Third" };
            var error = Error.Validation(messages);

            Assert.Equal("First", error.Message);
        }

        [Fact]
        public void Messages_IsReadOnly()
        {
            var error = Error.NotFound("Single message");

            var messagesCollection = error.Messages;
            Assert.NotNull(messagesCollection);
            Assert.IsAssignableFrom<IReadOnlyList<string>>(messagesCollection);
        }

        [Fact]
        public void HasMultipleMessages_WithSingleMessage_ReturnsFalse()
        {
            var error = Error.NotFound("Only one message");

            Assert.False(error.HasMultipleMessages);
        }

        [Fact]
        public void HasMultipleMessages_WithMultipleMessages_ReturnsTrue()
        {
            var error = Error.Validation(new[] { "Message 1", "Message 2" });

            Assert.True(error.HasMultipleMessages);
        }

        #endregion

        #region Append Operations

        [Fact]
        public void Append_WithSingleMessage_AddsMessageToExisting()
        {
            var error = Error.NotFound("Original message");
            var newMessage = "Additional message";

            var appendedError = error.Append(newMessage);

            Assert.Equal(2, appendedError.Messages.Count);
            Assert.Equal("Original message", appendedError.Messages[0]);
            Assert.Equal("Additional message", appendedError.Messages[1]);
            Assert.Equal(ErrorType.NotFound, appendedError.ErrorType);
        }

        [Fact]
        public void Append_WithMultipleMessages_AddsAllMessages()
        {
            var error = Error.Validation(new[] { "Message 1", "Message 2" });
            var newMessages = new[] { "Message 3", "Message 4" };

            var appendedError = error.Append(newMessages);

            Assert.Equal(4, appendedError.Messages.Count);
            Assert.Contains("Message 1", appendedError.Messages);
            Assert.Contains("Message 2", appendedError.Messages);
            Assert.Contains("Message 3", appendedError.Messages);
            Assert.Contains("Message 4", appendedError.Messages);
        }

        [Fact]
        public void Append_PreservesErrorType()
        {
            var error = Error.Conflict("Original");

            var appendedError = error.Append("Added");

            Assert.Equal(ErrorType.Conflict, appendedError.ErrorType);
        }

        [Fact]
        public void Append_PreservesException()
        {
            var exception = new Exception("Critical");
            var error = Error.InternalServerError("Original", exception);

            var appendedError = error.Append("Additional");

            Assert.Same(exception, appendedError.Exception);
        }

        #endregion

        #region ToString

        [Fact]
        public void ToString_WithSingleMessage_IncludesErrorTypeAndMessage()
        {
            var error = Error.NotFound("Resource not found");

            var stringRepresentation = error.ToString();

            Assert.Contains("NotFound", stringRepresentation);
            Assert.Contains("Resource not found", stringRepresentation);
        }

        [Fact]
        public void ToString_WithMultipleMessages_IncludesAllMessages()
        {
            var error = Error.Validation(new[] { "Error 1", "Error 2", "Error 3" });

            var stringRepresentation = error.ToString();

            Assert.Contains("Validation", stringRepresentation);
            Assert.Contains("Error 1", stringRepresentation);
            Assert.Contains("Error 2", stringRepresentation);
            Assert.Contains("Error 3", stringRepresentation);
        }

        [Fact]
        public void ToString_WithDifferentErrorTypes_DisplaysCorrectType()
        {
            var notFoundStr = Error.NotFound("msg").ToString();
            var unauthorizedStr = Error.Unauthorized("msg").ToString();
            var conflictStr = Error.Conflict("msg").ToString();
            var internalStr = Error.InternalServerError("msg").ToString();

            Assert.Contains("NotFound", notFoundStr);
            Assert.Contains("Unauthorized", unauthorizedStr);
            Assert.Contains("Conflict", conflictStr);
            Assert.Contains("InternalServerError", internalStr);
        }

        #endregion

        #region Equality and Records

        [Fact]
        public void TwoErrorsWithSameTypeAndMessage_HaveSameProperties()
        {
            var error1 = Error.NotFound("Same message");
            var error2 = Error.NotFound("Same message");

            Assert.Equal(error1.ErrorType, error2.ErrorType);
            Assert.Equal(error1.Message, error2.Message);
            Assert.Equal(error1.Messages.Count, error2.Messages.Count);
        }

        [Fact]
        public void TwoErrorsWithDifferentMessages_AreNotEqual()
        {
            var error1 = Error.NotFound("Message 1");
            var error2 = Error.NotFound("Message 2");

            Assert.NotEqual(error1, error2);
        }

        [Fact]
        public void TwoErrorsWithDifferentTypes_AreNotEqual()
        {
            var error1 = Error.NotFound("Same message");
            var error2 = Error.Validation("Same message");

            Assert.NotEqual(error1, error2);
        }

        #endregion
    }
}

