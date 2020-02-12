namespace Dashboard.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using SaaSFulfillmentClient.Models;

    [BindProperties]
    public class AzureSubscriptionProvisionModel
    {
        [Display(Name = "Available plans")]
        public IEnumerable<Plan> AvailablePlans { get; set; }

        [Display(Name = "Business unit contact email")]
        public string BusinessUnitContactEmail { get; set; }

        public string Email { get; set; }

        [Display(Name = "Subscriber full name")]
        public string FullName { get; set; }

        [BindProperty]
        public string NewPlanId { get; set; }

        [Display(Name = "Offer ID")]
        public string OfferId { get; set; }

        public bool PendingOperations { get; set; }

        [Display(Name = "Current plan")]
        public string PlanId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TargetContosoRegionEnum Region { get; set; }

        [Display(Name = "SaaS Subscription Id")]
        public Guid SubscriptionId { get; set; }

        [Display(Name = "Subscription name")]
        public string SubscriptionName { get; set; }

        public StatusEnum SubscriptionStatus { get; set; }

        [Display(Name = "Purchaser email")]
        public string PurchaserEmail { get; set; }

        [Display(Name = "Purchaser AAD TenantId")]
        public Guid PurchaserTenantId { get; set; }
    }
}