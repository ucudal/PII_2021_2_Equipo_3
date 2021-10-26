using System;

namespace Library
{
    /// <summary>
    /// This class represents a nullable value.
    /// </summary>
    /// <typeparam name="T">The type contained in the option.</typeparam>
    public struct Option<T>
    {
        /// <summary>
        /// Whether there is a value.
        /// </summary>
        /// <value></value>
        public bool HasValue { get; }

        private T value { get; }

        private Option(bool hasValue, T value)
        {
            this.HasValue = hasValue;
            this.value = value;
        }

        /// <summary>
        /// Returns an empty <see cref="Option{T}" />
        /// </summary>
        public static Option<T> None() =>
            new Option<T>(false, default);

        /// <summary>
        /// Returns an <see cref="Option{T}" /> with the given value, <br />
        /// or an empty one if the value's null.
        /// </summary>
        /// <param name="value">The given value.</param>
        public static Option<T> From(T value) =>
            new Option<T>(value is not null, value);

        /// <summary>
        /// Returns the result of a function which receives the given value if there is, <br />,
        /// or the result of a function which doesn't receive anything if there isn't.
        /// </summary>
        /// <param name="someFunc">The function for the success value.</param>
        /// <param name="noneFunc">The function for the error value.</param>
        /// <typeparam name="U">The type returned by the functions.</typeparam>
        public U Map<U>(Func<T, U> someFunc, Func<U> noneFunc) =>
            this.HasValue ? someFunc(this.value) : noneFunc();
        
        /// <summary>
        /// Attempts to retrieve the value from this <see cref="Option{T}" />, throwing an error if not possible.
        /// </summary>
        /// <exception cref="ArgumentNullException">This option doesn't have a value.</exception>
        /// <returns>The option's inner value.</returns>
        public T Unwrap() =>
            this.Map(
                v => v,
                () => throw new ArgumentNullException("value", "attempting to retrieve value from None")
            );
    }
}