namespace BeaconAPI.classes.DataBase
{
    internal class DatabaseSettings
    {
        public string DatabaseName { get; set; } = "";
        public string DatabasePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datalake");
    }
}
