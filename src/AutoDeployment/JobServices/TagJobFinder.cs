using AutoDeployment.Interfaces;
using AutoDeployment.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.JobServices
{
    public class TagJobFinder : CronJobService
    {
        ILogger<TagJobFinder> Logger { get; set; }
        IFinanceGitLabService GitLabClient { get; set; }
        IObjectStorage ObjectStorage { get; set; }
        public TagJobFinder(IScheduleConfig<TagJobFinder> cronConfig, ILogger<TagJobFinder> logger, IFinanceGitLabService gitLabClient, IObjectStorage objectStorage) : base(cronConfig.CronExpression, cronConfig.TimeZoneInfo)
        {
            Logger = logger;
            GitLabClient = gitLabClient;
            ObjectStorage = objectStorage;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting CronJobService: " + nameof(TagJobFinder));
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var singleConversation in ObjectStorage.ProjectVersions)
                {
                    foreach (var tagsToFind in singleConversation.Value)
                    {
                        var tagPipeline = await GitLabClient.FindLastTagPipeline(tagsToFind.ProjectId, tagsToFind.TagVersion);
                        if (tagPipeline != null)
                        {
                            var listProjectMerges = ObjectStorage.ReleaseMerges.Single(w => w.Key == singleConversation.Key).Value;
                            if (listProjectMerges.ProjectMerges.Any() && listProjectMerges.ProjectMerges.Any(s => s.GitProject.Id == tagsToFind.ProjectId))
                            {
                                listProjectMerges.ProjectMerges.Single(s => s.GitProject.Id == tagsToFind.ProjectId).GitTagPipeline = tagPipeline;
                                tagsToFind.PipelineFound = true;
                            }
                        }
                    }
                    var timeToRemove = DateTime.UtcNow + TimeSpan.FromMinutes(10);
                    singleConversation.Value.RemoveAll(r => r.PipelineFound || r.CreatedAt > timeToRemove);

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.StackTrace);
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stoping CronJobService: " + nameof(TagJobFinder));
            return base.StopAsync(cancellationToken);
        }
    }
}
