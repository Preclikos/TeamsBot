using AutoDeployment.Models.GitLab;
using AutoDeployment.Semver;
using GitLabApiClient.Models.Job.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using GitLabApiClient.Models.Pipelines.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface IFinanceGitLabService
    {
        Task<int> GetProjectInGroup();
        IEnumerable<(string Name, int Id)> GetProjectInGroup(int projectId);
        Task<string> GetLastProjectTag(int projectId);
        Task<SemVersion> GenerateNewProjectTag(int projectId, bool minor = false);
        Task<IEnumerable<GitProjectReleaseMerge>> GetReleaseMergeRequests(string conversationId, string label = "release", string version = null);
        Task<IEnumerable<Job>> GetMergeRequestPipelinesJobs(int projectId, int mergerRequestId);
        Task<ApprovalState> GetMergeRequestApproveState(int projectId, int mergeRequestIid);
        Task<bool> CheckBranchExist(int projectId, string brancheName, string versionName);
        Task<Pipeline> FindLastTagPipeline(int projectId, string tagName);
        Task<IEnumerable<IGrouping<string, Job>>> GetRunningPipelineJobs(int projectId, int pipelineId);
        List<(string Name, int GroupId)> GetTrackedGroups();
        Task<(string Name, string Version)> CreateReleaseOfGroup(int groupId);
    }
}
