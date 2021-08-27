using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AutoDeployment.Interfaces;
using AutoDeployment.Models;
using AutoDeployment.Models.CommandModels;
using GitLabApiClient.Models.MergeRequests.Responses;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoDeployment
{
    public class BotMainService : TeamsActivityHandler
    {
        private ILogger<BotMainService> Logger { get; set; }
        private ICommandService Commands { get; set; }
        private IReflectionRunner ReflectionRunner { get; set; }
        public BotMainService(ILogger<BotMainService> logger, ITokenStore tokenStore, ICommandService commands, IReflectionRunner reflectionRunner)
        {
            Logger = logger;
            Commands = commands;
            ReflectionRunner = reflectionRunner;
        }

        private async Task<bool> InvalidChannelId(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            if (string.IsNullOrEmpty(teamsChannelId) || teamsChannelId != "19:57cddd0c4244478492273dc99e29a0da@thread.skype")
            {
                var replyTextNotAllowed = $"This channel is not allowed!!";
                var activityReject = MessageFactory.Text(replyTextNotAllowed, replyTextNotAllowed);
                await turnContext.SendActivityAsync(activityReject, cancellationToken);
                return true;
            }

            return false;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Value != null)
            {
                await ActivityThread(turnContext, cancellationToken);
                return;
            }
            await MessageThread(turnContext, cancellationToken);

        }
        private async Task ActivityThread(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // This was a message from the card.
            var obj = (JObject)turnContext.Activity.Value;
            var props = obj.Properties();
            var commandParameters = props;
            var commandName = props.Single(f => f.Name == "command").Value.ToString();
            var subCommandName = props.Single(f => f.Name == "subCommand").Value.ToString();
            var uniqueId = props.Single(f => f.Name == "uniqueId").Value.ToString();

            Logger.LogWarning(string.Join(", ", obj.Properties().Select(s => s.Value.ToString()).ToArray()));

            if (Commands.CheckCommandExist(commandName))
            {
                await ReflectionRunner.RunCommand(turnContext, cancellationToken, commandName, subCommandName, uniqueId, obj);
                return;
            }

        }
        private async Task MessageThread(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var textCommand = turnContext.Activity.Text
                .Trim()
                .Replace("<at>finance bot</at> ", String.Empty, StringComparison.InvariantCultureIgnoreCase); //prevention remove bot name mention
            var textSubCommand = string.Empty;
            string[] textCommandParameters = null;
            Logger.LogInformation(textCommand);
            if(textCommand.Contains(' '))
            {
                var textCommandArray = textCommand.Split(' ');
                textCommand = textCommandArray[0];
                textSubCommand = textCommandArray[1].ToLower();

                var newArraySize = (textCommandArray.Length - 2);
                textCommandParameters = new string[newArraySize];
                Array.Copy(textCommandArray, 2, textCommandParameters, 0, textCommandParameters.Length);

                Logger.LogInformation("Spaces detected need Split. Splited result command: "+ textCommand);
            }

            textCommand = textCommand.ToLower();

            /*
            if((await InvalidChannelId(turnContext, cancellationToken)) && !textCommand.Contains("token"))
            {
                return;
            }*/

            if (Commands.CheckCommandExist(textCommand) && String.IsNullOrEmpty(textSubCommand))
            {
                var subCommands = Commands.GetSubCommands(textCommand).Where(w => !w.InternalOnly);
                await CardHelpers.SendSubCommandInfoCard(turnContext, textCommand, subCommands, cancellationToken);
            }

            if (Commands.CheckCommandExist(textCommand) && Commands.GetSubCommands(textCommand).Any(a => a.Name == textSubCommand && !a.InternalOnly))
            {
                await ReflectionRunner.RunCommand(turnContext, cancellationToken, textCommand, textSubCommand, null, textCommandParameters);  
            }

            //if it is not command do nothing
            // This is a regular text message.
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Hello from the TeamsMessagingExtensionsActionPreviewBot."), cancellationToken);
        }
    }
}