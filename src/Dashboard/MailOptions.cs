namespace Dashboard.Controllers
{
    public class MailOptions
    {
        public string AdminEmail { get; internal set; }
        public string FromEmail { get; internal set; }
        public string Host { get; internal set; }
        public string Password { get; internal set; }

        public int Port { get; internal set; }
        public string UserName { get; internal set; }
    }
}