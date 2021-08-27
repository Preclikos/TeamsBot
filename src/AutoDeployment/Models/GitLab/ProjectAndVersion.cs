using System;

namespace AutoDeployment.Models.GitLab
{
    public class ProjectTagVersion
    {
        public ProjectTagVersion(int projectId, string tagVersion)
        {
            ProjectId = projectId;
            TagVersion = tagVersion;
            CreatedAt = DateTime.UtcNow;
            PipelineFound = false;
        }
        public int ProjectId { get; set; }
        public string TagVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool PipelineFound { get; set; }
    }
}
