using System.Globalization;

namespace ProvaAPI.Classi
{
    public class IssueSemplice
    {
        public string idReadable { get; set; }
        public string summary { get; set; }
        public Value Fields { get; set; }

    }
    public class Value
    {
        public string priority { get; set; }
        public string assignee { get; set; }
        public string team { get; set; }
        public long dueDate { get; set; }
        public long startDate { get; set; }
        public string state { get; set; }
        public int workEffort { get; set; }
    }
}
