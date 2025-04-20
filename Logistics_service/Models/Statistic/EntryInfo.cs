namespace Logistics_service.Models.Statistic
{
    public class EntryInfo
    {
        public int Id { get; set; }

        public DateTime DateOfEntry { get; set; }

        public string IpAddress { get; set; }

        public EntryInfo()
        {

        }

        public EntryInfo(DateTime dateOfEntry, string ipAdress)
        {
            DateOfEntry = dateOfEntry;
            IpAddress = ipAdress;
        }
    }
}
