using System;
using System.Collections.Generic;
using AdaptiveCards;
using AutoDeployment.Models.CommandModels;

namespace AutoDeployment
{

    public static partial class CardHelpers
    {
        private static AdaptiveCard CreateWorkingCard(WorkingCardState cardState, string cardText)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"));
            AdaptiveColumnSet adaptiveColumnSet = new AdaptiveColumnSet();

            string imageUrl = String.Empty;

            switch (cardState)
            {
                case WorkingCardState.Working:
                    imageUrl = "https://financebot.preclikos.cz:4567/loading.png";
                    break;
                case WorkingCardState.Done:
                    imageUrl = "https://financebot.preclikos.cz:4567/done.png";
                    break;
            }

            AdaptiveImage adaptiveImage = new AdaptiveImage()
            {
                PixelWidth = 40,
                PixelHeight = 40
            };
            adaptiveImage.Url = new Uri(imageUrl);
            AdaptiveColumn adaptiveColumnImage = new AdaptiveColumn()
            {
                Items = new List<AdaptiveElement>() { adaptiveImage },
                Spacing = AdaptiveSpacing.Default,
                Width = "auto"
            };
            adaptiveColumnSet.Columns.Add(adaptiveColumnImage);

            AdaptiveTextBlock adaptiveText = new AdaptiveTextBlock()
            {
                Text = cardText,// "Working!!";
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Size = AdaptiveTextSize.Large
            };
            AdaptiveColumn adaptiveColumnText = new AdaptiveColumn()
            {
                Items = new List<AdaptiveElement>() { adaptiveText },
                Spacing = AdaptiveSpacing.Default,
                Width = "stretch",
                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center
            };
            adaptiveColumnSet.Columns.Add(adaptiveColumnText);

            adaptiveCard.Body.Add(adaptiveColumnSet);

            return adaptiveCard;
        }

        private static AdaptiveCard CreateSubCommandInfoCard(string commandName, IEnumerable<SubCommand> subCommands)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"));


            AdaptiveTextBlock adaptiveHeaderText = new AdaptiveTextBlock()
            {
                Text = "Usage of '" + commandName + "' command ",
                Size = AdaptiveTextSize.Medium,
            };
            adaptiveCard.Body.Add(adaptiveHeaderText);



            foreach (SubCommand subCommand in subCommands)
            {
                AdaptiveColumnSet adaptiveColumnSet = new AdaptiveColumnSet()
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = subCommand.Name,
                                Size = AdaptiveTextSize.Default,
                                HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                Wrap = true
                            }
                        },
                        Width = "2"
                        },
                        new AdaptiveColumn()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text = "- "+subCommand.Description,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                    Wrap = true
                                }
                            },
                            VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                            Width = "8"
                        }
                    }
                };

                adaptiveCard.Body.Add(adaptiveColumnSet);
            }

            return adaptiveCard;
        }
    }
}
