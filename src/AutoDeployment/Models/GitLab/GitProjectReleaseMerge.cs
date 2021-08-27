using GitLabApiClient.Models.Job.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using GitLabApiClient.Models.Pipelines.Responses;
using GitLabApiClient.Models.Projects.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoDeployment.Models.GitLab
{
    public class GitProjectReleaseMerge
    {
        public GitProjectReleaseMerge(Project project, MergeRequest mergeRequest, ApprovalState approvalState)
        {
            GitProject = project;
            GitMergeRequest = mergeRequest;
            GitMergeApprovalState = approvalState;
            GitPipelineJobs = new List<IGrouping<string, Job>>();
        }
        public Project GitProject { get; set; }
        public MergeRequest GitMergeRequest { get; set; }
        public ApprovalState GitMergeApprovalState { get; set; }
        public Pipeline GitTagPipeline { get; set; }
        public IEnumerable<IGrouping<string, Job>> GitPipelineJobs { get; set; }

        public bool ApprovalState()
        {
            var approvalState = this.GitMergeApprovalState;
            if (approvalState.Rules.Any() && 
                approvalState.Rules.Any(a => 
                !a.Approved && 
                a.ApprovalsRequired != 0))
            {
                 return false;
            }
            return true;
        }
    }
}
