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
    [BotService("release")]
    public partial class BotReleaseService
    {
        private IFinanceGitLabService FinanceGitLab { get; set; }
        private IFinanceBotGitLabService FinanceBotGitLab { get; set; }
        private IUserObjectStorage ObjectStorage { get; set; }
        private ILogger<BotReleaseService> Logger { get; set; }
        private IMessageInformationService MessageInformation { get; set; }
        public BotReleaseService(ILogger<BotReleaseService> logger, IFinanceGitLabService financeGitLab, IFinanceBotGitLabService financeBotGitLab, IMessageInformationService messageInformation, IUserObjectStorage objectStorage)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FinanceGitLab = financeGitLab ?? throw new ArgumentNullException(nameof(financeGitLab));
            FinanceBotGitLab = financeBotGitLab ?? throw new ArgumentNullException(nameof(financeBotGitLab));
            ObjectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
            MessageInformation = messageInformation ?? throw new ArgumentNullException(nameof(messageInformation));
        }

        [BotCommand("deploy", "Work with actualy prepared Relase, u can use with additional version parameter.")]
        public async Task ReleaseCall(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            string versionString = null;
            if (textCommandAttributes.Count() > 0)
            {
                versionString = textCommandAttributes[0];
            }
            var workMessage = await CardHelpers.SendMessage(turnContext, cancellationToken);
            var releaseMerges = await FinanceGitLab.GetReleaseMergeRequests(MessageInformation.ConversationContext.Conversation.Id, version: versionString);
            if (releaseMerges.Count() > 0)
            {
                //update
                await UpdateReleaseActivity(turnContext, releaseMerges, workMessage.Id, ObjectStorage.GitLabReleaseMerges.CreatedTime, cancellationToken);
                return;
            }

            //delete workMessage
            await turnContext.DeleteActivityAsync(workMessage.Id, cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text($"Nothing in release state!!"), cancellationToken);
        }

        [BotCommand("clean", null, true)]
        public async Task CleanReleaseBrancheMerge(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, ButtonClickData textCommandAttributes)
        {
            if (!ConcurrencyCheck(textCommandAttributes.TimeStamp))
            {
                await ConcurrencyExceptionMessage(turnContext, cancellationToken);
                return;
            }
            await CardHelpers.UpdateMessage(turnContext, uniqueMessageId, cancellationToken);

            await FinanceBotGitLab.CleanReleaseMergeRequests();
            await UpdateReleaseActivity(turnContext, ObjectStorage.GitLabReleaseMerges.ProjectMerges, uniqueMessageId, ObjectStorage.GitLabReleaseMerges.CreatedTime, cancellationToken);

        }

        [BotCommand("jobplay", null, true)]
        public async Task PlayTagJob(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, PlayActionButton textCommandAttributes)
        {
            if (!ConcurrencyCheck(textCommandAttributes.TimeStamp))
            {
                await ConcurrencyExceptionMessage(turnContext, cancellationToken);
                return;
            }

            await FinanceBotGitLab.ResumePipelineJob(textCommandAttributes.ProjectId, textCommandAttributes.JobId);
        }

        [BotCommand("create", "Create new release for specific group")]
        public async Task CreateRelease(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var workMessage = await CardHelpers.SendMessage(turnContext, cancellationToken);
            var groupList = FinanceGitLab.GetTrackedGroups();
            var singleProject = false;
            if (textCommandAttributes.Count() > 0 && textCommandAttributes[0].Contains("single"))
            {
                singleProject = true;
            }
            await UpdateReleaseGroupActivity(turnContext, workMessage.Id, groupList, singleProject, cancellationToken);
        }
        
        [BotCommand("groupcreate", null, true)]
        public async Task CreateReleaseOfGroup(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, ResponseCreateGroupRelease textCommandAttributes)
        {
            await CardHelpers.UpdateMessage(turnContext, uniqueMessageId, cancellationToken);
            var userId = await FinanceBotGitLab.GetCurrentUserId();
            var result = await FinanceGitLab.CreateReleaseOfGroup(textCommandAttributes.SelectedGroup);
            await UpdateReleaseGroupActivity(turnContext, uniqueMessageId, result.Name, result.Version, cancellationToken);

        }
        
        [BotCommand("listgroupproject", null, true)]
        public async Task CreateReleaseListProjectInGroup(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, ResponseCreateGroupRelease textCommandAttributes)
        {
            await CardHelpers.UpdateMessage(turnContext, uniqueMessageId, cancellationToken);
            var result = FinanceGitLab.GetProjectInGroup(textCommandAttributes.SelectedGroup);
            await UpdateReleaseGroupActivity(turnContext, uniqueMessageId, result, cancellationToken);

        }

        [BotCommand("approve", null, true)]
        public async Task ApproveReleaseMerge(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, ButtonClickData textCommandAttributes)
        {
            if (!ConcurrencyCheck(textCommandAttributes.TimeStamp))
            {
                await ConcurrencyExceptionMessage(turnContext, cancellationToken);
                return;
            }
            await CardHelpers.UpdateMessage(turnContext, uniqueMessageId, cancellationToken);

            await FinanceBotGitLab.ApproveReleaseMerge();
            await UpdateReleaseActivity(turnContext, ObjectStorage.GitLabReleaseMerges.ProjectMerges, uniqueMessageId, ObjectStorage.GitLabReleaseMerges.CreatedTime, cancellationToken);

        }

        [BotCommand("merge", null, true)]
        public async Task AcceptReleaseMerge(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, ButtonClickData textCommandAttributes)
        {
            if (!ConcurrencyCheck(textCommandAttributes.TimeStamp))
            {
                await ConcurrencyExceptionMessage(turnContext, cancellationToken);
                return;
            }
            await CardHelpers.UpdateMessage(turnContext, uniqueMessageId, cancellationToken);

            await FinanceBotGitLab.AcceptMergeRelease();
            ObjectStorage.GitLabReleaseMerges.TeamsId = uniqueMessageId;
            ObjectStorage.GitLabReleaseMerges.TeamsReference = turnContext.Activity.GetConversationReference();
            await UpdateReleaseActivity(turnContext, ObjectStorage.GitLabReleaseMerges.ProjectMerges, uniqueMessageId, ObjectStorage.GitLabReleaseMerges.CreatedTime, cancellationToken);

        }

        [BotCommand("reload", "Reload tracked projects.")]
        public async Task ReloadProjects(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var workMessage = await CardHelpers.SendMessage(turnContext, cancellationToken);
            var foundProjects = await FinanceGitLab.GetProjectInGroup();
            //update
            await CardHelpers.UpdateMessage(turnContext, workMessage.Id, cancellationToken, CardHelpers.WorkingCardState.Done, $"Successfully update {foundProjects}");
        }
        private async Task ConcurrencyExceptionMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var messageActivity = MessageFactory.Text("Merge requests was modified from other side cannot continue!");
            await turnContext.SendActivityAsync(messageActivity, cancellationToken);
        }

        private bool ConcurrencyCheck(long actualTimeStamp)
        {
            Logger.LogWarning(ObjectStorage.GitLabReleaseMerges.CreatedTime.ToString() + " | " + actualTimeStamp.ToString());
            if (ObjectStorage.GitLabReleaseMerges.CreatedTime == actualTimeStamp)
            {
                return true;
            }
            return false;
        }
    }
}
