namespace Dashboard.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using SaaSFulfillmentClient.Models;

    public class AzureSubscriptionProvisionModel
    {
        public IEnumerable<Plan> AvailablePlans { get; set; }
        public string Email { get; set; }

        public string FullName { get; set; }

        [Display(Name = "Maximum number of things to manage")]
        public int MaximumNumberOfThingsToHandle { get; set; }

        public string OfferId { get; set; }

        public string PlanName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TargetContosoRegionEnum Region { get; set; }

        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }
    }
}