using AutoDeployment.Attributes;
using AutoDeployment.Interfaces;
using AutoDeployment.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.BotServices
{
    [BotService("channel")]
    public class BotChannelInfo
    {
        private IProactiveBotService ProactiveBot { get; set; }
        public BotChannelInfo(IProactiveBotService proactiveBot)
        {
            ProactiveBot = proactiveBot;
        }

        [BotCommand("info", "Get information about current channel to other configuration")]
        public async Task GetChannelInfo(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var reference = turnContext.Activity.GetConversationReference();

            string resultMessage = "Bot Id: " + reference.Bot.Id + "\n" +
                "Channel Id: " + reference.ChannelId + "\n" +
                "Conversation Id: " + reference.Conversation.Id + " Tentand Id: " + reference.Conversation.TenantId + "\n" +
                "ServiceUrl: " + reference.ServiceUrl;

            await turnContext.SendActivityAsync(resultMessage, cancellationToken: cancellationToken);
        }

        [BotCommand("teams", "Get information about current Teams channel to other configuration")]
        public async Task GetTeamsChannelInfo(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();

            string resultMessage = "Teams Id: " + teamsChannelId;

            await turnContext.SendActivityAsync(resultMessage, cancellationToken: cancellationToken);
        }

        [BotCommand("member", "Get information about member")]
        public async Task GetTeamsMemberInfo(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var members = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);

            string resultMessage = "Member Id: " + members.First().Id;

            await turnContext.SendActivityAsync(resultMessage, cancellationToken: cancellationToken);
        }
        /*
        [BotCommand("set", "Set current channel to proactive Messaging")]
        public async Task SetChannelInfo(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            ProactiveChannelModel channelModel = new ProactiveChannelModel();
            channelModel.TeamsChannel = turnContext.Activity.GetChannelData<TeamsChannelData>();
            channelModel.ServiceUrl = turnContext.Activity.ServiceUrl;
            channelModel.TenantId = turnContext.Activity.Conversation.TenantId;
            channelModel.BotId = turnContext.Activity.Recipient;

            ProactiveBot.SaveChannel(channelModel);

            await turnContext.SendActivityAsync("Channel is successfully set and save", cancellationToken: cancellationToken);
        }*/

        [BotCommand("send", "Send test message to Reference")]
        public async Task TestSend(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {

            var testActivity = MessageFactory.Text("Test Proactive message");

            await ProactiveBot.SendMessage(testActivity, Enums.Channel.Name.Test_JHS  ,cancellationToken);

            await turnContext.SendActivityAsync("Message was send", cancellationToken: cancellationToken);
        }
    }
}
