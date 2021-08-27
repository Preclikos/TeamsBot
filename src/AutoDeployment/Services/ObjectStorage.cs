using AutoDeployment.Interfaces;
using AutoDeployment.Models.GitLab;
using System.Collections.Generic;
using System.Linq;

namespace AutoDeployment.Services
{
    public class ObjectStorage : IObjectStorage
    {
        public IDictionary<string, ListGitProjectReleaseMerge> ReleaseMerges { get; set; }
        public IDictionary<string, List<ProjectTagVersion>> ProjectVersions { get; set; }
        public ObjectStorage()
        {
            ReleaseMerges = new Dictionary<string, ListGitProjectReleaseMerge>();
            GitLabGroupProjects = new List<GroupProject>();
            ProjectVersions = new Dictionary<string, List<ProjectTagVersion>>();
        }
        //Tracking models
        public List<GroupProject> GitLabGroupProjects { get; set; }
    }
}
