using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GitLabApiClient.Models.MergeRequests.Responses
{
    public class ApprovalState
    {
        [JsonProperty("approval_rules_overwritten")]
        public bool ApprovalRulesOverwritten { get; set; }

        [JsonProperty("rules")]
        public List<ApprovalRule> Rules { get; } = new List<ApprovalRule>();
    }
}
