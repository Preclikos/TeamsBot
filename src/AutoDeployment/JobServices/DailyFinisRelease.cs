using AutoDeployment.Interfaces;
using AutoDeployment.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.JobServices
{
    public class DailyFinisRelease : CronJobService
    {
        ILogger<DailyFinisRelease> Logger { get; set; }
        IFinanceGitLabService GitLabClient { get; set; }
        private IProactiveBotService ProactiveBot { get; set; }
        private string AppId { get; set; }
        public DailyFinisRelease(IScheduleConfig<DailyFinisRelease> cronConfig, ILogger<DailyFinisRelease> logger, IFinanceGitLabService gitLabClient, IProactiveBotService proactiveBot, IConfiguration configuration) : base(cronConfig.CronExpression, cronConfig.TimeZoneInfo)
        {
            Logger = logger;
            GitLabClient = gitLabClient;
            AppId = configuration.GetValue<string>("MicrosoftAppId");
            ProactiveBot = proactiveBot;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting CronJobService: " + nameof(DailyFinisRelease));
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                await GitLabClient.CreateReleaseOfGroup(160);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.StackTrace);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stoping CronJobService: " + nameof(DailyFinisRelease));
            return base.StopAsync(cancellationToken);
        }
    }
}
