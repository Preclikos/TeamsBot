using AutoDeployment.Interfaces;
using AutoDeployment.Models.GitLab;
using GitLabApiClient;
using GitLabApiClient.Models.Branches.Requests;
using GitLabApiClient.Models.MergeRequests.Requests;
using GitLabApiClient.Models.MergeRequests.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AutoDeployment.Services
{
    public class FinanceBotGitLabService : IFinanceBotGitLabService
    {
        private ILogger<FinanceBotGitLabService> Logger { get; set; }
        private IUserObjectStorage ObjectStorage { get; set; }
        private GitLabOptions GitOptions { get; set; }
        private GitLabClient GitClient { get; set; }
        private ITokenStore TokenStore { get; set; }

        public FinanceBotGitLabService(ILogger<FinanceBotGitLabService> logger, ITokenStore tokenStore, IOptions<GitLabOptions> gitOptions, IUserObjectStorage objectStorage)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            GitOptions = gitOptions.Value ?? throw new ArgumentNullException(nameof(gitOptions.Value));
            TokenStore = tokenStore ?? throw new ArgumentNullException(nameof(gitOptions.Value));
            ObjectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
            GitClient = new GitLabClient(GitOptions.ServerUrl, tokenStore.GetToken());

        }

        public async Task CleanReleaseMergeRequests()
        {
            var emptyMergeRequests = ObjectStorage.GitLabReleaseMerges.ProjectMerges.Where(w =>
                w.GitMergeRequest.Status == MergeStatus.CannotBeMerged &&
                w.GitMergeRequest.ChangesCount == null);

            foreach (MergeRequest mergeRequest in emptyMergeRequests.Select(s => s.GitMergeRequest))
            {
                if (mergeRequest.Status == MergeStatus.CannotBeMerged && mergeRequest.ChangesCount == null)
                {
                    UpdateMergeRequest updateMerge = new UpdateMergeRequest()
                    {
                        State = RequestedMergeRequestState.Close
                    };
                    await GitClient.MergeRequests.UpdateAsync(mergeRequest.ProjectId, mergeRequest.Iid, updateMerge);
                    await GitClient.Branches.DeleteBranch(mergeRequest.ProjectId, HttpUtility.UrlEncode(mergeRequest.SourceBranch));

                    mergeRequest.State = MergeRequestState.Closed;
                }

            }

        }

        public async Task ApproveReleaseMerge()
        {
            var mergesToApprove = ObjectStorage.GitLabReleaseMerges.ProjectMerges.Where(w =>
                    w.GitMergeRequest.Status == MergeStatus.CanBeMerged &&
                    w.ApprovalState() == false);
            foreach (var mergeRequest in mergesToApprove)

            {
                await GitClient.MergeRequests.ApproveAsync(mergeRequest.GitMergeRequest.ProjectId, mergeRequest.GitMergeRequest.Iid);
                mergeRequest.GitMergeApprovalState = await GitClient.MergeRequests.GetApprovalStateAsync(mergeRequest.GitMergeRequest.ProjectId, mergeRequest.GitMergeRequest.Iid);
            }
        }

        public async Task ResumePipelineJob(int projectId, int jobId)
        {
            await GitClient.Pipelines.PlayJobAsync(projectId, jobId);
        }

        public async Task<IEnumerable<ProjectTagVersion>> AcceptMergeRelease()
        {
            foreach (var mergeRequest in ObjectStorage.GitLabReleaseMerges.ProjectMerges.Where(w => w.GitMergeRequest.Status == MergeStatus.CanBeMerged).Select(s => s.GitMergeRequest))
            {

                AcceptMergeRequest acceptMerge = new AcceptMergeRequest()
                {
                    RemoveSourceBranch = true
                };

                await GitClient.MergeRequests.AcceptAsync(mergeRequest.ProjectId, mergeRequest.Iid, acceptMerge);

                string[] version = mergeRequest.SourceBranch.Split('/');

                Logger.LogWarning(version[1]);

                ObjectStorage.GitLabProjectVersions.Add(
                    new ProjectTagVersion(
                        Convert.ToInt32(mergeRequest.ProjectId),
                        version[1])
                    );

                mergeRequest.State = MergeRequestState.Merged;

            }

            return ObjectStorage.GitLabProjectVersions;
        }

        public async Task<MergeRequest> CreateReleaseBrancheWithMergerRequest(int projectId, string versionName)
        {
            CreateBranchRequest createBranch = new CreateBranchRequest("release/" + versionName, "develop");
            var resultBranche = await GitClient.Branches.CreateAsync(projectId, createBranch);

            CreateMergeRequest createMerge = new CreateMergeRequest("release/" + versionName, "master", "Release " + versionName);

            var mergeRequest = await GitClient.MergeRequests.CreateAsync(projectId, createMerge);
            return mergeRequest;
        }
        public async Task<int> GetCurrentUserId()
        {
            var user = await GitClient.Users.GetCurrentSessionAsync();
            return user.Id;
        }
        public async Task<int> FindWrongOwnMerges()
        {
            var random = new Random();
            var tokenList = TokenStore.GetAllOtherTokens().ToList();

            var currentClient = await GitClient.Users.GetCurrentSessionAsync();
            var userMergeRequest = await GitClient.MergeRequests.GetAsync(options => { options.AuthorId = currentClient.Id; options.State = QueryMergeRequestState.All; });

            var noUpVoted = userMergeRequest.Where(w => w.Upvotes == 0 && w.TargetBranch == "develop");
            foreach (var toUpVote in noUpVoted)
            {
                int index = random.Next(tokenList.Count);
                var selectedToken = tokenList[index];

                var UserIndependGitClient = new GitLabClient(GitOptions.ServerUrl, selectedToken);
                try
                {
                    await UserIndependGitClient.MergeRequests.AwardEmoji(toUpVote.ProjectId, toUpVote.Iid);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Cannot approve ThumbsUp Id " + toUpVote.Iid + " - " + ex.Message);
                }
                if (toUpVote.State == MergeRequestState.Merged)
                {
                    try
                    {
                        await UserIndependGitClient.MergeRequests.ApproveAsync(toUpVote.ProjectId, toUpVote.Iid);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning("Cannot approve Merge Id " + toUpVote.Iid + " - " + ex.Message);
                    }
                }
            }
            return noUpVoted.Count();
        }
    }
}