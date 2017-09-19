namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public class BuildingTarget
    {
        public static readonly BuildingTarget Body = new BuildingTarget("Body", true, true);
        public static readonly BuildingTarget Form = new BuildingTarget("Form", false, true);
        public static readonly BuildingTarget Header = new BuildingTarget("Header", true, true);
        public static readonly BuildingTarget Query = new BuildingTarget("Query", false, true);

        public BuildingTarget(string id, bool isGreedy, bool isToRequest) : this(id, id, isGreedy, isToRequest)
        {
        }

        public BuildingTarget(string id, string displayName, bool isGreedy, bool isToRequest)
        {
            Id = id;
            DisplayName = displayName;
            IsGreedy = isGreedy;
            IsToRequest = isToRequest;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public bool IsGreedy { get; }
        public bool IsToRequest { get; }
    }
}