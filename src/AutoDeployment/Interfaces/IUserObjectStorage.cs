using AutoDeployment.Models.GitLab;
using System.Collections.Generic;

namespace AutoDeployment.Interfaces
{
    public interface IUserObjectStorage
    {
        List<GroupProject> GitLabGroupProjects { get; }
        ListGitProjectReleaseMerge GitLabReleaseMerges { get; }
        List<ProjectTagVersion> GitLabProjectVersions { get; }
    }
}
