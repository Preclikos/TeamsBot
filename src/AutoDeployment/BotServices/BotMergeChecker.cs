using AutoDeployment.Attributes;
using AutoDeployment.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.BotServices
{
    [BotService("merge")]
    public class BotMergeChecker
    {
        private IFinanceBotGitLabService FinanceBotGitLab { get; set; }
        private ILogger<BotMergeChecker> Logger { get; set; }
        public BotMergeChecker(ILogger<BotMergeChecker> logger, IFinanceBotGitLabService financeBotGitLab)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FinanceBotGitLab = financeBotGitLab ?? throw new ArgumentNullException(nameof(financeBotGitLab));
        }
        [BotCommand("approve", "Approve all unapproved or unThumbed MRs.")]
        public async Task CheckWrongMerges(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var workMessage = await CardHelpers.SendMessage(turnContext, cancellationToken);
            try
            {
                
                var releaseMerges = await FinanceBotGitLab.FindWrongOwnMerges();

                await CardHelpers.UpdateMessage(turnContext, workMessage.Id, cancellationToken, cardText: "Success Thumbed up and Approve " + releaseMerges);
            }catch
            {
                await turnContext.DeleteActivityAsync(workMessage.Id, cancellationToken);
            }

        }
    }
}
