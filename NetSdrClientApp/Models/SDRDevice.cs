namespace NetSdrClientApp.Models
{
    public class SDRDevice
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsConnected { get; set; }

        public override string ToString()
        {
            return Name ?? "Unnamed Device";
        }
    }
}
