using AutoDeployment.BotServices;
using AutoDeployment.Interfaces;
using AutoDeployment.Models.GitLab;
using AutoDeployment.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.JobServices
{
    public class PipelineTracker : CronJobService
    {
        ILogger<PipelineTracker> Logger { get; set; }
        protected readonly IObjectStorage ObjectStorage;

        protected readonly IFinanceGitLabService GitLabClient;

        private BotAdapter TeamsBotAdapter { get; set; }
        private string AppId { get; set; }

        private ListGitProjectReleaseMerge ActualToSend { get; set; }

        public PipelineTracker(IScheduleConfig<PipelineTracker> cronConfig, ILogger<PipelineTracker> logger, IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IFinanceGitLabService gitLabClient, IObjectStorage objectStorage) : base(cronConfig.CronExpression, cronConfig.TimeZoneInfo)
        {
            GitLabClient = gitLabClient;
            ObjectStorage = objectStorage;
            Logger = logger;
            AppId = configuration.GetValue<string>("MicrosoftAppId");
            TeamsBotAdapter = (BotAdapter)adapter;

            
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting CronJobService: "+nameof(PipelineTracker));
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {

            try
            {
                if (ObjectStorage.ReleaseMerges.Values.Any(a => a.TeamsReference != null))
                {
                    //Get Any ReleaseMerges not depend on conversationId
                    var projectMergesToWork = ObjectStorage.ReleaseMerges.Where(w => w.Value.ProjectMerges.Any(w => w.GitTagPipeline != null));
                    foreach (var gitProjectRelease in projectMergesToWork)
                    {
                        //in specific conversation get merges with Pipeline if some found found
                        var pipelinesToWork = gitProjectRelease.Value.ProjectMerges.Where(w => w.GitTagPipeline != null);
                        if (pipelinesToWork.Any())
                        {
                            foreach (var runningPipeline in pipelinesToWork)
                            {
                                var status = true;
                                if (runningPipeline.GitPipelineJobs != null && runningPipeline.GitPipelineJobs.Any())
                                {
                                    status = runningPipeline.GitPipelineJobs.Any(s => s.Any(ss => ss.Status == "running" || ss.Stage == "created" || ss.Status == "pending" || ss.Status == "manual"));
                                }
                                Logger.LogWarning("Status is " + status + " for " + runningPipeline.GitMergeRequest.Title);
                                if (status)
                                {
                                    runningPipeline.GitPipelineJobs = await GitLabClient.GetRunningPipelineJobs(runningPipeline.GitProject.Id, runningPipeline.GitTagPipeline.Id);
                                }
                                else
                                {
                                    runningPipeline.GitTagPipeline = null;
                                }
                            }
                            ActualToSend = gitProjectRelease.Value;
                            await TeamsBotAdapter.ContinueConversationAsync(AppId, gitProjectRelease.Value.TeamsReference, BotCallback, default(CancellationToken));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.StackTrace);
            }
        }
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await BotReleaseService.UpdateReleaseActivity(turnContext, ActualToSend.ProjectMerges, ActualToSend.TeamsId, ActualToSend.CreatedTime, cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stoping CronJobService: " + nameof(PipelineTracker));
            return base.StopAsync(cancellationToken);
        }
    }
}
