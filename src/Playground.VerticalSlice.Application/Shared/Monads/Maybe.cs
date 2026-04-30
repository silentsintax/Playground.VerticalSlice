namespace Playground.VerticalSlice.Application.Shared.Monads
{
    public readonly struct Maybe<T> : IEquatable<Maybe<T>>
    {
        private readonly T? _value;
        private readonly bool _isSome;

        public bool IsSome => _isSome;

        public bool IsNone => !_isSome;

        private Maybe(T? value, bool isSome)
        {
            _value = value;
            _isSome = isSome;
        }

        public static Maybe<T> Some(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Use None() instead of Some(null)");

            return new Maybe<T>(value, true);
        }

        public static Maybe<T> None() => new Maybe<T>(default, false);

        public static Maybe<T> FromNullable(T? value) =>
            value != null ? Some(value) : None();

       
        public Maybe<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return _isSome ? Maybe<TResult>.Some(selector(_value!)) : Maybe<TResult>.None();
        }

        public Maybe<TResult> FlatMap<TResult>(Func<T, Maybe<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return _isSome ? selector(_value!) : Maybe<TResult>.None();
        }

        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> selector) =>
            FlatMap(selector);

        public Maybe<T> Tap(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (_isSome)
                action(_value!);

            return this;
        }

        public Maybe<T> TapNone(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (!_isSome)
                action();

            return this;
        }

        public T GetOrElse(T defaultValue) =>
            _isSome ? _value! : defaultValue;

        public T GetOrElse(Func<T> defaultSelector)
        {
            if (defaultSelector == null)
                throw new ArgumentNullException(nameof(defaultSelector));

            return _isSome ? _value! : defaultSelector();
        }

        public T GetOrThrow() =>
            GetOrThrow(() => new InvalidOperationException("Cannot get value from None"));

        public T GetOrThrow(Func<Exception> exceptionSelector)
        {
            if (exceptionSelector == null)
                throw new ArgumentNullException(nameof(exceptionSelector));

            if (!_isSome)
                throw exceptionSelector();

            return _value!;
        }

        public T? ToNullable() => _value;

        public Maybe<T> Where(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _isSome && predicate(_value!) ? this : None();
        }

        public IEnumerable<T> ToEnumerable()
        {
            if (_isSome)
                yield return _value!;
        }

        public void Match(Action<T> onSome, Action onNone)
        {
            if (onSome == null)
                throw new ArgumentNullException(nameof(onSome));
            if (onNone == null)
                throw new ArgumentNullException(nameof(onNone));

            if (_isSome)
                onSome(_value!);
            else
                onNone();
        }

        public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
        {
            if (onSome == null)
                throw new ArgumentNullException(nameof(onSome));
            if (onNone == null)
                throw new ArgumentNullException(nameof(onNone));

            return _isSome ? onSome(_value!) : onNone();
        }

        public bool Equals(Maybe<T> other)
        {
            if (_isSome != other._isSome)
                return false;

            return !_isSome || EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object? obj) =>
            obj is Maybe<T> other && Equals(other);

        public override int GetHashCode() =>
            _isSome ? EqualityComparer<T>.Default.GetHashCode(_value!) : 0;

        public override string ToString() =>
            _isSome ? $"Some({_value})" : "None";

        public static bool operator ==(Maybe<T> left, Maybe<T> right) =>
            left.Equals(right);

        public static bool operator !=(Maybe<T> left, Maybe<T> right) =>
            !left.Equals(right);

        public static implicit operator Maybe<T>(T value) =>
            Some(value);
    }

    /// <summary>
    /// Provides factory methods for creating Maybe instances.
    /// </summary>
    public static class Maybe
    {
        public static Maybe<T> Some<T>(T value) => Maybe<T>.Some(value);
        public static Maybe<T> None<T>() => Maybe<T>.None();
        public static Maybe<T> FromNullable<T>(T? value) => Maybe<T>.FromNullable(value);
    }
}
