using System.Collections.Generic;
using SaaSFulfillmentClient.Models;

namespace Dashboard.Models
{
    public class UpdateSubscriptionViewModel
    {
        public IEnumerable<Plan> AvailablePlans { get;  set; }
        public string CurrentPlan { get;  set; }
        public string NewPlan { get; set; }
    }
}