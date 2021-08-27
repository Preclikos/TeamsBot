using AutoDeployment.Models.GitLab;
using GitLabApiClient.Models.MergeRequests.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface IFinanceBotGitLabService
    {
        Task<MergeRequest> CreateReleaseBrancheWithMergerRequest(int projectId, string versionName);
        Task CleanReleaseMergeRequests();
        Task ApproveReleaseMerge();
        Task<IEnumerable<ProjectTagVersion>> AcceptMergeRelease();
        Task ResumePipelineJob(int projectId, int jobId);
        Task<int> FindWrongOwnMerges();
        Task<int> GetCurrentUserId();
    }
}
