using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GitLabApiClient.Models.MergeRequests.Responses
{
    public class ApprovalRule
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rule_type")]
        public string RuleType { get; set; }

        [JsonProperty("approved_by")]
        public List<ApprovalUser> ApprovedBy { get; } = new List<ApprovalUser>();

        [JsonProperty("approved")]
        public bool Approved { get; set; }

        [JsonProperty("approvals_required")]
        public int ApprovalsRequired { get; set; }

    }
}
