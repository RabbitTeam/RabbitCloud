using Rabbit.Cloud.Client.Go.Filters;

namespace Rabbit.Cloud.Client.Go
{
    public class GoOptions
    {
        public GoOptions()
        {
            DefaultScheme = "http";
            Filters = new FilterCollection();
        }

        public string DefaultScheme { get; set; }
        public FilterCollection Filters { get; }
    }
}