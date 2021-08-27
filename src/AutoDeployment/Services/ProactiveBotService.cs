using AutoDeployment.Enums;
using AutoDeployment.Interfaces;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.Services
{
    public class ProactiveBotService : IProactiveBotService
    {
        private string AppId { get; }
        private string AppPassword { get; }
        private ProactiveBotOptions BotOptions { get; }

        public ProactiveBotService(IConfiguration configuration, IOptions<ProactiveBotOptions> botOptions)
        {
            AppId = configuration.GetValue<string>("MicrosoftAppId");
            AppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
            BotOptions = botOptions.Value ?? throw new ArgumentNullException(nameof(botOptions));
        }

        public async Task<string> SendMessage(Activity activityToSend, Channel.Name channelName, CancellationToken cancellationToken)
        {
            var channelId = Channel.GetId(channelName);

            MicrosoftAppCredentials.TrustServiceUrl(BotOptions.ServiceUrl, DateTime.MaxValue);

            var credentials = new MicrosoftAppCredentials(AppId, AppPassword);
            ConnectorClient _client = new ConnectorClient(new Uri(BotOptions.ServiceUrl), credentials, new HttpClient());

            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                Bot = new ChannelAccount() { Id = BotOptions.BotId, Name = BotOptions.BotName },
                ChannelData = new TeamsChannelData() { Channel =  new ChannelInfo() { Id = channelId }, Team = new TeamInfo() { Id = BotOptions.TeamId }, Tenant = new TenantInfo() { Id = BotOptions.TenantId} },
                TenantId = BotOptions.TenantId,
                Activity = activityToSend
            };


            var response = await _client.Conversations.CreateConversationAsync(conversationParameters);
            return response.ActivityId;

        }
    }
}