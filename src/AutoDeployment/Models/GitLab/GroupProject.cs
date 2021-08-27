using GitLabApiClient.Models.Groups.Responses;
using GitLabApiClient.Models.Projects.Responses;

namespace AutoDeployment.Models.GitLab
{
    public class GroupProject
    {
        public GroupProject(Group group, Project project)
        {
            GitGroup = group;
            GitProject = project;
        }
        public Group GitGroup { get; set; }
        public Project GitProject { get; set; }
    }
}
