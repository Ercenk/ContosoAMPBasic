using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dashboard.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using SaaSFulfillmentClient.Models;

    [BindProperties]
    public class AzureSubscriptionProvisionModel
    {
        [Display(Name = "Available plans")]
        public IEnumerable<Plan> AvailablePlans { get; set; }

        public string Email { get; set; }

        [Display(Name = "Subscriber full name")]
        public string FullName { get; set; }

        [Display(Name = "Maximum number of things to manage")]
        public int MaximumNumberOfThingsToHandle { get; set; }

        [BindProperty]
        public string NewPlan { get; set; }

        [Display(Name = "Offer ID")]
        public string OfferId { get; set; }

        public bool PendingOperations { get; set; }

        [Display(Name = "Current plan")]
        public string PlanName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TargetContosoRegionEnum Region { get; set; }

        [Display(Name = "SaaS Subscription Id")]
        public Guid SubscriptionId { get; set; }

        [Display(Name = "Subscription name")]
        public string SubscriptionName { get; set; }
    }
}