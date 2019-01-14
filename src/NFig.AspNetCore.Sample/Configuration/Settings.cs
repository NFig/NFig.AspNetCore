namespace NFig.AspNetCore.Sample.Configuration
{
    public partial class Settings : INFigSettings<Tier, DataCenter>
    {
        public string ApplicationName { get; set; }
        public string Commit { get; set; }
        public Tier Tier { get; set; }
        public DataCenter DataCenter { get; set; }
    }
}
