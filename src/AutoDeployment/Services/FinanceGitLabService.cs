using AutoDeployment.Interfaces;
using AutoDeployment.Models.GitLab;
using AutoDeployment.Semver;
using GitLabApiClient;
using GitLabApiClient.Models;
using GitLabApiClient.Models.Job.Responses;
using GitLabApiClient.Models.MergeRequests.Requests;
using GitLabApiClient.Models.MergeRequests.Responses;
using GitLabApiClient.Models.Pipelines.Responses;
using GitLabApiClient.Models.Projects.Responses;
using GitLabApiClient.Models.Tags.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Services
{
    public class FinanceGitLabService : IFinanceGitLabService
    {
        private ILogger<FinanceGitLabService> Logger { get; set; }
        private IObjectStorage ObjectStorage { get; set; }
        private GitLabClient GitClient { get; set; }
        private GitLabOptions GitOptions { get; set; }
        public FinanceGitLabService(ILogger<FinanceGitLabService> logger, IOptions<GitLabOptions> gitOptions, IObjectStorage objectStorage)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            GitOptions = gitOptions.Value ?? throw new ArgumentNullException(nameof(gitOptions.Value));
            GitClient = new GitLabClient(GitOptions.ServerUrl, GitOptions.AdminToken);
            ObjectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));

            //Need load Project on Init
            if (!ObjectStorage.GitLabGroupProjects.Any())
            {
                var projectFind = GetProjectInGroup();
                projectFind.Wait();
            }
        }
        public IEnumerable<(string Name, int Id)> GetProjectInGroup(int projectId)
        {
            return ObjectStorage.GitLabGroupProjects.Where(w => w.GitGroup.Id == projectId).Select(s => (s.GitProject.Name, s.GitProject.Id));
        }

        public async Task<int> GetProjectInGroup()
        {
            ObjectStorage.GitLabGroupProjects = new List<GroupProject>();
            if (GitOptions.DedicatedProjectIds.Length > 0)
            {
                foreach (int projectId in GitOptions.DedicatedProjectIds)
                {
                    var dedicatedProject = await GitClient.Projects.GetAsync(projectId);
                    if (dedicatedProject.Namespace.Kind == "group")
                    {
                        var dedicatedProjectGroup = await GitClient.Groups.GetAsync(dedicatedProject.Namespace.Id);
                        ObjectStorage.GitLabGroupProjects.Add(new GroupProject(dedicatedProjectGroup, dedicatedProject));
                        Logger.LogInformation($"Found dedicated project { dedicatedProject.Name } in Group {dedicatedProjectGroup.Name}.");
                    }

                }
            }

            int groupId = GitOptions.TrackedGroupId;

            var group = await GitClient.Groups.GetAsync(groupId);
            var projects = await GitClient.Groups.GetProjectsAsync(groupId);
            foreach (Project project in projects)
            {
                ObjectStorage.GitLabGroupProjects.Add(new GroupProject(group, project));
            }
            Logger.LogInformation($"Found {projects.Count()} in primary group {group.Name}.");


            var subGroups = await GitClient.Groups.GetSubgroupsAsync(groupId);
            Logger.LogInformation($"Found {subGroups.Count()} subGroups in primary Group.");

            foreach (var subGroup in subGroups)
            {
                var subGroupToAdd = await GitClient.Groups.GetProjectsAsync(subGroup.Id);

                foreach (Project project in subGroupToAdd)
                {
                    ObjectStorage.GitLabGroupProjects.Add(new GroupProject(subGroup, project));
                }
                Logger.LogInformation($"Found {subGroupToAdd.Count()} subGroups in Group {subGroup.Name}.");

            }

            return ObjectStorage.GitLabGroupProjects.Count;
        }

        public async Task<string> GetLastProjectTag(int projectId)
        {
            var projectTags = await GitClient.Tags.GetAsync(projectId);
            if (projectTags.Any())
            {
                var sortedTagsByRelease = projectTags.OrderByDescending(o => o.Commit.CreatedAt);
                return sortedTagsByRelease.First().Name;
            }
            return "0.0.0";
        }


        public async Task<ApprovalState> GetMergeRequestApproveState(int projectId, int mergeRequestIid)
        {
            return await GitClient.MergeRequests.GetApprovalStateAsync(projectId, mergeRequestIid);
        }

        public async Task<SemVersion> GenerateNewProjectTag(int projectId, bool incrementPatchVersion = false)
        {
            var lastFindedTag = await GetLastProjectTag(projectId);
            var version = SemVersion.Parse(lastFindedTag);

            var newVersion = incrementPatchVersion ? version.IncrementPatch() : version.IncrementMinor();

            return newVersion.ToString();
        }

        public async Task<IEnumerable<GitProjectReleaseMerge>> GetReleaseMergeRequests(string conversationId, string label = "release", string version = null)
        {
            List<GitProjectReleaseMerge> projectsAndMerges = new List<GitProjectReleaseMerge>();
            foreach (Project project in ObjectStorage.GitLabGroupProjects.Select(s => s.GitProject))
            {
                var projectID = project.Id;
                var releaseMergeRequest = await GitClient.MergeRequests.GetAsync(
                    projectID,
                    options =>
                    {
                        options.Labels.Add(label);
                        options.State = QueryMergeRequestState.Opened;
                        options.Order = MergeRequestsOrder.CreatedAt;
                        options.SortOrder = SortOrder.Descending;
                    });

                if (!String.IsNullOrEmpty(version))
                {
                    string versionWithBranche = label + "/" + version;
                    releaseMergeRequest = releaseMergeRequest.Where(w => w.SourceBranch == versionWithBranche).ToList();
                }

                if (releaseMergeRequest.Any())
                {
                    var selected = releaseMergeRequest.OrderBy(d => d.CreatedAt).First();
                    projectsAndMerges.Add(
                        new GitProjectReleaseMerge(
                            project,
                            await GitClient.MergeRequests.GetChangesAsync(projectID, selected.Iid),
                            await GitClient.MergeRequests.GetApprovalStateAsync(projectID, selected.Iid)
                            ));
                }

            }
            ObjectStorage.ReleaseMerges.Single(s => s.Key == conversationId).Value.ProjectMerges = projectsAndMerges;
            return projectsAndMerges;
        }

        public async Task<IEnumerable<Job>> GetMergeRequestPipelinesJobs(int projectId, int mergerRequestId)
        {
            var currentMergerRequest = await GitClient.MergeRequests.GetAsync(projectId, options => { options.MergeRequestsIds = new List<int>() { mergerRequestId }; });
            var currentPipeline = await GitClient.Pipelines.GetAsync(projectId, options => { options.Sha = currentMergerRequest.Single().Sha; options.Ref = currentMergerRequest.Single().SourceBranch; });
            var currentJobs = await GitClient.Pipelines.GetJobsAsync(projectId, currentPipeline.Single().Id);

            return currentJobs;
        }

        public async Task<IEnumerable<IGrouping<string, Job>>> GetRunningPipelineJobs(int projectId, int pipelineId)
        {
            var jobs = await GitClient.Pipelines.GetJobsAsync(projectId, pipelineId);
            return jobs.GroupBy(g => g.Stage);
        }

        public List<(string Name, int GroupId)> GetTrackedGroups()
        {
            var groupList = new List<(string Name, int ProjectId)>();
            foreach (var group in ObjectStorage.GitLabGroupProjects.GroupBy(g => g.GitGroup.Id))
            {
                groupList.Add((group.First().GitGroup.Name, group.Key));
            }
            return groupList;
        }

        public async Task<(string Name, string Version)> CreateReleaseOfGroup(int groupId)
        {
            var projects = ObjectStorage.GitLabGroupProjects.Where(w => w.GitGroup.Id == groupId);
            var groupName = projects.First().GitGroup.Name;

            var projectNewVersions = await Task.WhenAll(projects.Select(p => GenerateNewProjectTag(p.GitProject.Id, false)));
            var finalVersion = projectNewVersions.Max().ToString();

            foreach (var project in projects)
            {
                var tagRequest = new CreateTagRequest(finalVersion, "master", finalVersion, "Bot AutoTag from master branch");
                await GitClient.Tags.CreateAsync(project.GitProject, tagRequest);
            }

            return (groupName, finalVersion);
        }

        public async Task<bool> CheckBranchExist(int projectId, string brancheName, string versionName)
        {
            try
            {
                await GitClient.Branches.GetAsync(projectId, brancheName + "%2F" + versionName);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<Pipeline> FindLastTagPipeline(int projectId, string tagName)
        {

            var pipelines = await GitClient.Pipelines.GetAsync(projectId, options => { options.Ref = tagName; options.SortOrder = SortOrder.Descending; });
            if (pipelines.Count > 0)
            {
                return pipelines.FirstOrDefault();
            }
            return null;
        }
    }
}
