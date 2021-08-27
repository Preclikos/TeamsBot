using AutoDeployment.Models.GitLab;
using System.Collections.Generic;

namespace AutoDeployment.Interfaces
{
    public interface IObjectStorage
    {
        IDictionary<string, ListGitProjectReleaseMerge> ReleaseMerges { get; set; }
        IDictionary<string, List<ProjectTagVersion>> ProjectVersions { get; set; }
        List<GroupProject> GitLabGroupProjects { get; set; }
    }
}
