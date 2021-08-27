using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment
{
    public class GitLabOptions
    {
        public string ServerUrl { get; set; }
        public string AdminToken { get; set; }
        public int TrackedGroupId { get; set; }
        public int[] DedicatedProjectIds { get; set; }
    }
}
