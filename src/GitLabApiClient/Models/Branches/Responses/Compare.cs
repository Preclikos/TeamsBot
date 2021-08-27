using System;
using System.Collections.Generic;
using System.Text;
using GitLabApiClient.Models.Releases.Responses;
using Newtonsoft.Json;

namespace GitLabApiClient.Models.Branches.Responses
{
    public sealed class Compare
    {
        [JsonProperty("commit")]
        public CommitDetail Commit { get; set; }
        [JsonProperty("commits")]
        public CommitDetail[] Commits { get; set; }
    }
}
