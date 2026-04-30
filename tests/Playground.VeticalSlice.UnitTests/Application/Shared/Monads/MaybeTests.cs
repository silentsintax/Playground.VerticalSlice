using Xunit;
using AutoFixture;
using Playground.VerticalSlice.Application.Shared.Monads;

namespace Playground.VeticalSlice.UnitTests.Application.Shared.Monads
{
    public class MaybeTests
    {
        private readonly IFixture _fixture = new Fixture();

        #region Creation

        [Fact]
        public void Some_WithValidValue_CreatesMaybeWithValue()
        {
            var value = _fixture.Create<string>();
            var maybe = Maybe.Some(value);

            Assert.True(maybe.IsSome);
            Assert.False(maybe.IsNone);
            Assert.Equal(value, maybe.GetOrElse(string.Empty));
        }

        [Fact]
        public void Some_WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Maybe.Some<string>(null!));
        }

        [Fact]
        public void None_CreatesEmptyMaybe()
        {
            var maybe = Maybe.None<int>();

            Assert.False(maybe.IsSome);
            Assert.True(maybe.IsNone);
        }

        [Fact]
        public void FromNullable_WithNonNullValue_CreatesSome()
        {
            var value = _fixture.Create<string>();
            var maybe = Maybe.FromNullable(value);

            Assert.True(maybe.IsSome);
            Assert.Equal(value, maybe.GetOrElse(string.Empty));
        }

        [Fact]
        public void FromNullable_WithNullValue_CreatesNone()
        {
            var maybe = Maybe.FromNullable<string>(null);

            Assert.True(maybe.IsNone);
        }

        #endregion

        #region Map Operations

        [Fact]
        public void Map_OnSome_AppliesFunctionAndReturnsSome()
        {
            var initial = Maybe.Some(5);

            var mapped = initial.Map(x => x * 2);

            Assert.True(mapped.IsSome);
            Assert.Equal(10, mapped.GetOrElse(0));
        }

        [Fact]
        public void Map_OnNone_ReturnsNoneWithoutApplyingFunction()
        {
            var maybe = Maybe.None<int>();
            var functionCalled = false;

            var mapped = maybe.Map(x => { functionCalled = true; return x * 2; });

            Assert.False(functionCalled);
            Assert.True(mapped.IsNone);
        }

        [Fact]
        public void Map_ChangeType_ReturnsCorrectType()
        {
            var maybe = Maybe.Some(5);

            var mapped = maybe.Map(x => x.ToString());

            Assert.True(mapped.IsSome);
            Assert.Equal("5", mapped.GetOrElse(string.Empty));
        }

        [Fact]
        public void Map_WithNullSelector_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(5);

            Assert.Throws<ArgumentNullException>(() => maybe.Map<int>(null!));
        }

        [Fact]
        public void Map_ChainMultipleMappings_AppliesAllTransformations()
        {
            var maybe = Maybe.Some(5);

            var result = maybe
                .Map(x => x * 2)
                .Map(x => x + 3)
                .Map(x => x.ToString());

            Assert.True(result.IsSome);
            Assert.Equal("13", result.GetOrElse(string.Empty));
        }

        #endregion

        #region FlatMap and Bind Operations

        [Fact]
        public void FlatMap_OnSomeReturningFlatMap_ReturnsInnerMaybe()
        {
            var maybe = Maybe.Some(5);

            var result = maybe.FlatMap(x => Maybe.Some(x * 2));

            Assert.True(result.IsSome);
            Assert.Equal(10, result.GetOrElse(0));
        }

        [Fact]
        public void FlatMap_OnSomeReturningNone_ReturnsNone()
        {
            var maybe = Maybe.Some(5);

            var result = maybe.FlatMap(x => Maybe.None<int>());

            Assert.True(result.IsNone);
        }

        [Fact]
        public void FlatMap_OnNone_ReturnsNoneWithoutApplyingFunction()
        {
            var maybe = Maybe.None<int>();
            var functionCalled = false;

            var result = maybe.FlatMap(x => { functionCalled = true; return Maybe.Some(x * 2); });

            Assert.False(functionCalled);
            Assert.True(result.IsNone);
        }

        [Fact]
        public void FlatMap_WithNullSelector_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(5);

            Assert.Throws<ArgumentNullException>(() => maybe.FlatMap<int>(null!));
        }

        [Fact]
        public void Bind_EquivalentToFlatMap()
        {
            var maybe = Maybe.Some(5);

            var flatMapResult = maybe.FlatMap(x => Maybe.Some(x * 2));
            var bindResult = maybe.Bind(x => Maybe.Some(x * 2));

            Assert.Equal(flatMapResult, bindResult);
        }

        [Fact]
        public void FlatMap_ChainOperations_StopsAtFirstNone()
        {
            var maybe = Maybe.Some(5);

            var result = maybe
                .FlatMap(x => Maybe.Some(x * 2))
                .FlatMap(x => Maybe.None<int>())
                .FlatMap(x => Maybe.Some(x + 10));

            Assert.True(result.IsNone);
        }

        #endregion

        #region Tap Operations

        [Fact]
        public void Tap_OnSome_ExecutesActionAndReturnsMaybe()
        {
            var maybe = Maybe.Some(5);
            var sideEffectExecuted = false;
            int capturedValue = 0;

            var result = maybe.Tap(x => { sideEffectExecuted = true; capturedValue = x; });

            Assert.True(sideEffectExecuted);
            Assert.Equal(5, capturedValue);
            Assert.True(result.IsSome);
            Assert.Equal(maybe, result);
        }

        [Fact]
        public void Tap_OnNone_SkipsActionAndReturnsMaybe()
        {
            var maybe = Maybe.None<int>();
            var sideEffectExecuted = false;

            var result = maybe.Tap(x => sideEffectExecuted = true);

            Assert.False(sideEffectExecuted);
            Assert.True(result.IsNone);
        }

        [Fact]
        public void Tap_WithNullAction_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(5);

            Assert.Throws<ArgumentNullException>(() => maybe.Tap(null!));
        }

        [Fact]
        public void TapNone_OnNone_ExecutesActionAndReturnsMaybe()
        {
            var maybe = Maybe.None<int>();
            var sideEffectExecuted = false;

            var result = maybe.TapNone(() => sideEffectExecuted = true);

            Assert.True(sideEffectExecuted);
            Assert.True(result.IsNone);
        }

        [Fact]
        public void TapNone_OnSome_SkipsActionAndReturnsMaybe()
        {
            var maybe = Maybe.Some(5);
            var sideEffectExecuted = false;

            var result = maybe.TapNone(() => sideEffectExecuted = true);

            Assert.False(sideEffectExecuted);
            Assert.True(result.IsSome);
        }

        [Fact]
        public void TapNone_WithNullAction_ThrowsArgumentNullException()
        {
            var maybe = Maybe.None<int>();

            Assert.Throws<ArgumentNullException>(() => maybe.TapNone(null!));
        }

        #endregion

        #region GetOrElse Operations

        [Fact]
        public void GetOrElse_WithValue_ReturnsSomeValue()
        {
            var maybe = Maybe.Some(10);

            var value = maybe.GetOrElse(42);

            Assert.Equal(10, value);
        }

        [Fact]
        public void GetOrElse_WithNone_ReturnsDefaultValue()
        {
            var maybe = Maybe.None<int>();

            var value = maybe.GetOrElse(42);

            Assert.Equal(42, value);
        }

        [Fact]
        public void GetOrElse_WithFuncAndValue_ReturnsSomeValue()
        {
            var maybe = Maybe.Some(10);

            var value = maybe.GetOrElse(() => 42);

            Assert.Equal(10, value);
        }

        [Fact]
        public void GetOrElse_WithFuncAndNone_ReturnsComputedValue()
        {
            var maybe = Maybe.None<int>();

            var value = maybe.GetOrElse(() => 42);

            Assert.Equal(42, value);
        }

        [Fact]
        public void GetOrElse_WithFuncNullSelector_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => maybe.GetOrElse((Func<int>)null!));
        }

        [Fact]
        public void GetOrElse_WithFuncNone_ExecutesSelector()
        {
            var maybe = Maybe.None<int>();
            var selectorCalled = false;

            maybe.GetOrElse(() => { selectorCalled = true; return 42; });

            Assert.True(selectorCalled);
        }

        #endregion

        #region GetOrThrow Operations

        [Fact]
        public void GetOrThrow_OnSome_ReturnsValue()
        {
            var maybe = Maybe.Some(10);

            var value = maybe.GetOrThrow();

            Assert.Equal(10, value);
        }

        [Fact]
        public void GetOrThrow_OnNone_ThrowsInvalidOperationException()
        {
            var maybe = Maybe.None<int>();

            Assert.Throws<InvalidOperationException>(() => maybe.GetOrThrow());
        }

        [Fact]
        public void GetOrThrow_WithCustomException_OnNone_ThrowsCustomException()
        {
            var maybe = Maybe.None<int>();
            var customException = new ArgumentException("Custom error");

            var exception = Assert.Throws<ArgumentException>(() => 
                maybe.GetOrThrow(() => customException));

            Assert.Same(customException, exception);
        }

        [Fact]
        public void GetOrThrow_WithNullExceptionSelector_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => maybe.GetOrThrow((Func<Exception>)null!));
        }

        [Fact]
        public void GetOrThrow_WithCustomExceptionOnSome_ReturnsValueWithoutThrow()
        {
            var maybe = Maybe.Some(10);

            var value = maybe.GetOrThrow(() => new Exception("Should not be thrown"));

            Assert.Equal(10, value);
        }

        #endregion

        #region ToNullable

        [Fact]
        public void ToNullable_OnSome_ReturnsValue()
        {
            var maybe = Maybe.Some("test");

            var value = maybe.ToNullable();

            Assert.Equal("test", value);
        }

        [Fact]
        public void ToNullable_OnNone_ReturnsNull()
        {
            var maybe = Maybe.None<string>();

            var value = maybe.ToNullable();

            Assert.Null(value);
        }

        #endregion

        #region Where (Filtering)

        [Fact]
        public void Where_OnSomeWithTruePredicate_ReturnsSome()
        {
            var maybe = Maybe.Some(10);

            var result = maybe.Where(x => x > 5);

            Assert.True(result.IsSome);
            Assert.Equal(10, result.GetOrElse(0));
        }

        [Fact]
        public void Where_OnSomeWithFalsePredicate_ReturnsNone()
        {
            var maybe = Maybe.Some(3);

            var result = maybe.Where(x => x > 5);

            Assert.True(result.IsNone);
        }

        [Fact]
        public void Where_OnNone_ReturnsNone()
        {
            var maybe = Maybe.None<int>();

            var result = maybe.Where(x => x > 5);

            Assert.True(result.IsNone);
        }

        [Fact]
        public void Where_WithNullPredicate_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => maybe.Where(null!));
        }

        [Fact]
        public void Where_ChainMultipleFilters_OnlyReturnsSomeIfAllConditionsMet()
        {
            var maybe = Maybe.Some(15);

            var result = maybe
                .Where(x => x > 10)
                .Where(x => x < 20)
                .Where(x => x % 3 == 0);

            Assert.True(result.IsSome);
        }

        [Fact]
        public void Where_ChainFilters_ReturnsNoneIfAnyConditionFails()
        {
            var maybe = Maybe.Some(15);

            var result = maybe
                .Where(x => x > 10)
                .Where(x => x < 20)
                .Where(x => x % 2 == 0);

            Assert.True(result.IsNone);
        }

        #endregion

        #region ToEnumerable

        [Fact]
        public void ToEnumerable_OnSome_ReturnsEnumerableWithValue()
        {
            var maybe = Maybe.Some(42);

            var enumerable = maybe.ToEnumerable();
            var list = enumerable.ToList();

            Assert.Single(list);
            Assert.Equal(42, list[0]);
        }

        [Fact]
        public void ToEnumerable_OnNone_ReturnsEmptyEnumerable()
        {
            var maybe = Maybe.None<int>();

            var enumerable = maybe.ToEnumerable();
            var list = enumerable.ToList();

            Assert.Empty(list);
        }

        #endregion

        #region Match Operations

        [Fact]
        public void Match_WithActionOnSome_ExecutesSomeAction()
        {
            var maybe = Maybe.Some(10);
            var someCalled = false;
            var noneCalled = false;

            maybe.Match(
                onSome: _ => someCalled = true,
                onNone: () => noneCalled = true
            );

            Assert.True(someCalled);
            Assert.False(noneCalled);
        }

        [Fact]
        public void Match_WithActionOnNone_ExecutesNoneAction()
        {
            var maybe = Maybe.None<int>();
            var someCalled = false;
            var noneCalled = false;

            maybe.Match(
                onSome: _ => someCalled = true,
                onNone: () => noneCalled = true
            );

            Assert.False(someCalled);
            Assert.True(noneCalled);
        }

        [Fact]
        public void Match_WithActionNullSomeFunc_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => 
                maybe.Match(null!, () => { }));
        }

        [Fact]
        public void Match_WithActionNullNoneFunc_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => 
                maybe.Match(_ => { }, (Action)null!));
        }

        [Fact]
        public void Match_WithFuncOnSome_ReturnsSomeResult()
        {
            var maybe = Maybe.Some(10);

            var result = maybe.Match(
                onSome: x => x * 2,
                onNone: () => -1
            );

            Assert.Equal(20, result);
        }

        [Fact]
        public void Match_WithFuncOnNone_ReturnsNoneResult()
        {
            var maybe = Maybe.None<int>();

            var result = maybe.Match(
                onSome: x => x * 2,
                onNone: () => -1
            );

            Assert.Equal(-1, result);
        }

        [Fact]
        public void Match_WithFuncNullSomeFunc_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => 
                maybe.Match<int>(null!, () => -1));
        }

        [Fact]
        public void Match_WithFuncNullNoneFunc_ThrowsArgumentNullException()
        {
            var maybe = Maybe.Some(10);

            Assert.Throws<ArgumentNullException>(() => 
                maybe.Match(x => x, (Func<int>)null!));
        }

        #endregion

        #region Equality and Operators

        [Fact]
        public void Equality_TwoSomeWithSameValue_AreEqual()
        {
            var maybe1 = Maybe.Some(42);
            var maybe2 = Maybe.Some(42);

            Assert.Equal(maybe1, maybe2);
            Assert.True(maybe1 == maybe2);
            Assert.False(maybe1 != maybe2);
        }

        [Fact]
        public void Equality_TwoSomeWithDifferentValue_AreNotEqual()
        {
            var maybe1 = Maybe.Some(42);
            var maybe2 = Maybe.Some(43);

            Assert.NotEqual(maybe1, maybe2);
            Assert.False(maybe1 == maybe2);
            Assert.True(maybe1 != maybe2);
        }

        [Fact]
        public void Equality_SomeAndNone_AreNotEqual()
        {
            var some = Maybe.Some(42);
            var none = Maybe.None<int>();

            Assert.NotEqual(some, none);
        }

        [Fact]
        public void Equality_TwoNone_AreEqual()
        {
            var none1 = Maybe.None<int>();
            var none2 = Maybe.None<int>();

            Assert.Equal(none1, none2);
        }

        [Fact]
        public void GetHashCode_SameValues_ProduceSameHash()
        {
            var maybe1 = Maybe.Some(42);
            var maybe2 = Maybe.Some(42);

            Assert.Equal(maybe1.GetHashCode(), maybe2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_TwoNone_ProduceSameHash()
        {
            var none1 = Maybe.None<int>();
            var none2 = Maybe.None<int>();

            Assert.Equal(none1.GetHashCode(), none2.GetHashCode());
        }

        #endregion

        #region ToString

        [Fact]
        public void ToString_OnSome_ReturnsFormattedString()
        {
            var maybe = Maybe.Some(42);

            var str = maybe.ToString();

            Assert.Contains("Some", str);
            Assert.Contains("42", str);
        }

        [Fact]
        public void ToString_OnNone_ReturnsNoneString()
        {
            var maybe = Maybe.None<int>();

            var str = maybe.ToString();

            Assert.Equal("None", str);
        }

        #endregion

        #region Factory Methods (Static)

        [Fact]
        public void StaticSome_CreatesMaybeWithValue()
        {
            var value = _fixture.Create<string>();
            var maybe = Maybe.Some(value);

            Assert.True(maybe.IsSome);
            Assert.Equal(value, maybe.GetOrElse(string.Empty));
        }

        [Fact]
        public void StaticNone_CreatesEmptyMaybe()
        {
            var maybe = Maybe.None<int>();

            Assert.True(maybe.IsNone);
        }

        [Fact]
        public void StaticFromNullable_WithValue_CreatesSome()
        {
            var value = _fixture.Create<string>();
            var maybe = Maybe.FromNullable(value);

            Assert.True(maybe.IsSome);
        }

        [Fact]
        public void StaticFromNullable_WithNull_CreatesNone()
        {
            var maybe = Maybe.FromNullable<string>(null);

            Assert.True(maybe.IsNone);
        }

        #endregion
    }
}

