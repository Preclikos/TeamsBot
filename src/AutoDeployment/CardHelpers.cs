using AdaptiveCards;
using AdaptiveCards.Templating;
using AutoDeployment.Models.CommandModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment
{
    public static partial class CardHelpers
    {
        public async static Task<ResourceResponse> UpdateOrSendActivity(ITurnContext turnContext, IMessageActivity messageActivity, bool isSend, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId.Contains("emulator") || isSend)
            {
                return await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }
            return await turnContext.UpdateActivityAsync(messageActivity, cancellationToken);
        }

        public async static Task<ResourceResponse> UpdateOrSendActivity(ITurnContext<IMessageActivity> turnContext, IMessageActivity messageActivity, bool isSend, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId.Contains("emulator") || isSend)
            {
                return await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }
            return await turnContext.UpdateActivityAsync(messageActivity, cancellationToken);
        }

        public static IMessageActivity ConvertToActivity(AdaptiveCard adaptiveAction, string prevousActionId = null)
        {
            var attachement = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveAction,
            };
            var replyContent = MessageFactory.Attachment(attachement);
            replyContent.Id = prevousActionId;
            return replyContent;
        }

        public static AdaptiveCard ConvertToAdaptiveCard(List<string> bodyElements, List<string> actionElements, string prevousActionId = null)
        {
            var bodyJsonElements = string.Empty;
            if (bodyElements.Any())
            {
                bodyJsonElements = string.Join(", ", bodyElements);
            }

            var actionJsonElements = string.Empty;
            if (actionElements.Any())
            {
                actionJsonElements = "\"actions\": [" + string.Join(", ", actionElements) + "],";
            }

            var cardEnvelopeBody = @"
            {
                ""type"": ""AdaptiveCard"",
                ""body"": [" + bodyJsonElements + @"],
                " + actionJsonElements + @"
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""version"": ""1.0""
            }";

            var resultCard = AdaptiveCard.FromJson(cardEnvelopeBody);
            resultCard.Card.Id = prevousActionId;
            return resultCard.Card;
        }

        public static string TransformTemplateToData(string template, string data)
        {
            var transformer = new AdaptiveTransformer();
            return transformer.Transform(template, data);
        }

        public static async Task<ResourceResponse> SendMessage(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, WorkingCardState cardState = WorkingCardState.Working, string cardText = "Working!!!")
        {
            var attachement = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = CreateWorkingCard(cardState, cardText),
            };
            var replyContent = MessageFactory.Attachment(attachement);
            return await turnContext.SendActivityAsync(replyContent, cancellationToken);
        }

        public static async Task<ResourceResponse> UpdateMessage(ITurnContext<IMessageActivity> turnContext, string activityId, CancellationToken cancellationToken, WorkingCardState cardState = WorkingCardState.Working, string cardText = "Working!!!")
        {
            var attachement = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = CreateWorkingCard(cardState, cardText),
            };
            var replyContent = MessageFactory.Attachment(attachement);
            replyContent.Id = activityId;
            return await UpdateOrSendActivity(turnContext, replyContent, false, cancellationToken);
        }

        public static async Task<ResourceResponse> SendSubCommandInfoCard(ITurnContext<IMessageActivity> turnContext, string commandName, IEnumerable<SubCommand> subCommands, CancellationToken cancellationToken)
        {
            var attachement = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = CreateSubCommandInfoCard(commandName, subCommands),
            };
            var replyContent = MessageFactory.Attachment(attachement);
            return await UpdateOrSendActivity(turnContext, replyContent, true, cancellationToken);
        }

        public enum WorkingCardState
        {
            Working = 1,
            Done = 2,
            Error = 3
        };
    }
}
