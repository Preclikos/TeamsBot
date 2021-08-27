using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitLabApiClient.Models.Branches.Responses
{
    public sealed class CommitDetail
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("short_id")]
        public string ShortId { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("author_name")]
        public string AuthorName { get; set; }
        [JsonProperty("author_email")]
        public string AuthorEmail { get; set; }
    }
}
