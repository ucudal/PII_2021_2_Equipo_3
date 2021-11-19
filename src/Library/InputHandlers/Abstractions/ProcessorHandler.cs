using System;
using Library.Core.Processing;

namespace Library.InputHandlers.Abstractions
{
    /// <summary>
    /// Represents an <see cref="IInputHandler" /> which uses an input processor, and a function to be given its result.
    /// </summary>
    public class ProcessorHandler : IInputHandler
    {
        private Func<string, Result<bool, string>> inputHandler;
        private readonly Func<string> initialResponseGetter;
        private Action resetter;

        ///
        private ProcessorHandler(Func<string, Result<bool, string>> inputHandler, Func<string> initialResponseGetter, Action resetter)
        {
            this.inputHandler = inputHandler;
            this.initialResponseGetter = initialResponseGetter;
            this.resetter = resetter;
        }

        /// <summary>
        /// Creates an instance of <see cref="ProcessorHandler" />.
        /// </summary>
        /// <param name="f">The operation to do with the resulting input, and returns a not-null string if there was an error.</param>
        /// <param name="processor">The input processor.</param>
        /// <typeparam name="T">The type of the object the input processor returns, which is used by the "action" operation.</typeparam>
        public static ProcessorHandler CreateInstance<T>(Func<T, string> f, IInputProcessor<T> processor)
        {
            return new ProcessorHandler (
                inputHandler: s => processor.GenerateFromInput(s).Map(
                    result => result.AndThen(
                        v =>
                        {
                            string res = f(v);
                            if(res == null) return Result<bool, string>.Ok(true);
                            processor.Reset();
                            return Result<bool, string>.Err($"{res}\n{processor.GetDefaultResponse()}");
                        }
                    ),
                    () => Result<bool, string>.Ok(false)
                ),
                initialResponseGetter: processor.GetDefaultResponse,
                resetter: processor.Reset
            );
        }

        /// <summary>
        /// Creates an instance of <see cref="ProcessorHandler" /> which can't send an error
        /// in the function which gets the generated value.
        /// </summary>
        /// <param name="action">The operation to do with the resulting input.</param>
        /// <param name="processor">The input processor.</param>
        /// <typeparam name="T">The type of the object the input processor returns, which is used by the "action" operation.</typeparam>
        public static ProcessorHandler CreateInfallibleInstance<T>(Action<T> action, IInputProcessor<T> processor) =>
            ProcessorHandler.CreateInstance<T>(v => {
                action(v);
                return null;
            }, processor);

        string IInputHandler.GetDefaultResponse() => (this.initialResponseGetter)();

        Result<bool, string> IInputHandler.ProcessInput(string msg) => (this.inputHandler)(msg);

        void IInputHandler.Reset() => (this.resetter)();
    }
}