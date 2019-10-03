namespace Dashboard
{
    using Dashboard.Mail;

    public class DashboardOptions
    {
        public bool AdvancedFlow { get; set; }

        public string BasePlanId { get; set; }

        public string BaseUrl { get; set; }

        public string DashboardAdmin { get; set; }

        public MailOptions Mail { get; set; }

        public bool ShowUnsubscribed { get; set; }
    }
}