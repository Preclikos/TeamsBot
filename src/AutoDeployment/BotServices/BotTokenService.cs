using AutoDeployment.Attributes;
using AutoDeployment.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.BotServices
{
    [BotService("token")]
    public class BotTokenService
    {
        private ITokenStore TokenStore { get; set; }
        public BotTokenService(ITokenStore tokenStore)
        {
            TokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        }

        [BotCommand("set", "Save personal GitLab token to store.", false)]
        public async Task SaveToken(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            if (!TokenStore.HasToken() && textCommandAttributes != null)
            {
                if (textCommandAttributes[0].Length == 20 || textCommandAttributes[0].Length == 64)
                {
                    TokenStore.SaveToken(textCommandAttributes[0]);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Personal token successfully saved."), cancellationToken);
                    return;
                }
                await turnContext.SendActivityAsync(MessageFactory.Text($"Wrong token!"), cancellationToken);
                return;
            }
            await turnContext.SendActivityAsync(MessageFactory.Text($"Personal token already exist!"), cancellationToken);
        }
        [BotCommand("get", "Get personal GitLab token from store.", false)]
        public async Task GetToken(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var token = TokenStore.GetToken();
            if (!String.IsNullOrEmpty(token))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Your GitLab token is: " + token), cancellationToken);
                return;
            }
            await turnContext.SendActivityAsync(MessageFactory.Text($"Dont have any token in token store!"), cancellationToken);
        }
        [BotCommand("check", "Check if exist personal GitLab token in store.", false)]
        public async Task CheckToken(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var tokenBool = TokenStore.HasToken();
            if (tokenBool)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Found some token is store for you."), cancellationToken);
                return;
            }
            await turnContext.SendActivityAsync(MessageFactory.Text($"Dont have any token in token store!"), cancellationToken);
        }

        [BotCommand("delete", "Delete your personal token from store if exist.", false)]
        public async Task DeleteToken(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string uniqueMessageId, string[] textCommandAttributes)
        {
            var tokenBool = TokenStore.HasToken();
            if (tokenBool)
            {
                TokenStore.DeleteToken();
                await turnContext.SendActivityAsync(MessageFactory.Text($"Your token deleted."), cancellationToken);
                return;
            }
            await turnContext.SendActivityAsync(MessageFactory.Text($"Dont have any token in token store!"), cancellationToken);
        }
    }
}
