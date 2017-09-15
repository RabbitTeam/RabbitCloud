namespace Rabbit.Cloud.Facade.Abstractions.ModelBinding
{
    public class BindingSource
    {
        public static readonly BindingSource Body = new BindingSource("Body", true, true);
        public static readonly BindingSource Form = new BindingSource("Form", false, true);
        public static readonly BindingSource Header = new BindingSource("Header", true, true);
        public static readonly BindingSource Query = new BindingSource("Query", false, true);

        public BindingSource(string id, bool isGreedy, bool isFromRequest)
        {
            Id = id;
            DisplayName = id;
            IsGreedy = isGreedy;
            IsFromRequest = isFromRequest;
        }

        public BindingSource(string id, string displayName, bool isGreedy, bool isFromRequest)
        {
            Id = id;
            DisplayName = displayName;
            IsGreedy = isGreedy;
            IsFromRequest = isFromRequest;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public bool IsGreedy { get; }
        public bool IsFromRequest { get; }
    }
}