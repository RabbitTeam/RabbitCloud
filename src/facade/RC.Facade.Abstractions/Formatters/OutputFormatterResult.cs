using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public class OutputFormatterResult
    {
        private static readonly OutputFormatterResult _failure = new OutputFormatterResult(true);
        private static readonly OutputFormatterResult _noValue = new OutputFormatterResult(false);
        private static readonly Task<OutputFormatterResult> _failureAsync = Task.FromResult(_failure);
        private static readonly Task<OutputFormatterResult> _noValueAsync = Task.FromResult(_noValue);

        private OutputFormatterResult(bool hasError)
        {
            HasError = hasError;
        }

        public OutputFormatterResult(object model)
        {
            Model = model;
            IsModelSet = true;
        }

        public bool HasError { get; }
        public bool IsModelSet { get; }
        public object Model { get; }

        public static OutputFormatterResult Failure()
        {
            return _failure;
        }

        public static Task<OutputFormatterResult> FailureAsync()
        {
            return _failureAsync;
        }

        public static OutputFormatterResult Success(object model)
        {
            return new OutputFormatterResult(model);
        }

        public static Task<OutputFormatterResult> SuccessAsync(object model)
        {
            return Task.FromResult(Success(model));
        }

        public static OutputFormatterResult NoValue()
        {
            return _noValue;
        }

        public static Task<OutputFormatterResult> NoValueAsync()
        {
            return _noValueAsync;
        }
    }
}