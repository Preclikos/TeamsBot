using AutoDeployment.Interfaces;
using AutoDeployment.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AutoDeployment.JobServices
{
    public class FinisReleaseRandomUserSelect : CronJobService
    {
        ILogger<FinisReleaseRandomUserSelect> Logger { get; set; }
        private IProactiveBotService ProactiveBot { get; }
        public FinisReleaseRandomUserSelect(IScheduleConfig<FinisReleaseRandomUserSelect> cronConfig, ILogger<FinisReleaseRandomUserSelect> logger, IProactiveBotService proactiveBot) : base(cronConfig.CronExpression, cronConfig.TimeZoneInfo)
        {
            Logger = logger;
            ProactiveBot = proactiveBot;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting CronJobService: " + nameof(FinisReleaseRandomUserSelect));
            return base.StartAsync(cancellationToken);
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        { 
            try
            {
                var randomUserId = Enums.User.GetRandom();
                var userObject = Enums.User.GetId(randomUserId);
                var mention = new Mention
                {
                    Mentioned = userObject,
                    Text = $"<at>{userObject.Name}</at>",
                };

                var replyActivity = MessageFactory.Text($"Today selected user for Release is {mention.Text}.");
                replyActivity.Entities = new List<Entity> { mention };

                await ProactiveBot.SendMessage(replyActivity, Enums.Channel.Name.Releases, cancellationToken);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.StackTrace);
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stoping CronJobService: " + nameof(FinisReleaseRandomUserSelect));
            return base.StopAsync(cancellationToken);
        }
    }
}
