namespace EventManagementSystem.Configuration
{
    public class EmailSettings
    {
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPassword { get; set; }
        public bool UseSsl { get; set; }
    }
}
