using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Models.GitLab
{
    public class ListGitProjectReleaseMerge
    {
        public ListGitProjectReleaseMerge()
        {
            ProjectMerges = new List<GitProjectReleaseMerge>();
            CreatedTime = DateTime.UtcNow.Ticks / 1000;
        }
        public List<GitProjectReleaseMerge> ProjectMerges { get; set; }
        public long CreatedTime { get; set; }
        public string TeamsId { get; set; }
        public ConversationReference TeamsReference { get; set; }
    }
}
