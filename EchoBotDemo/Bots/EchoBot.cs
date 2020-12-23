// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EchoBotDemo.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.Equals(turnContext.Activity.Text, "wait", System.StringComparison.InvariantCultureIgnoreCase))
            {
                await turnContext.SendActivitiesAsync(
                    new Activity[] {
                new Activity { Type = ActivityTypes.Typing }, // 正在輸入的狀態
                new Activity { Type = "delay", Value= 15000 }, //等待15秒
                MessageFactory.Text("Finished typing", "Finished typing"),  //回應的結果
                MessageFactory.Text("Finished typing2", "Finished typing2"),
                MessageFactory.Text("Finished typing3", "Fi1nished typing3"),
                    },
                    cancellationToken);
            }
            else if (string.Equals(turnContext.Activity.Text, "image", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var reply = MessageFactory.Text("This is an inline attachment with file.");
                reply.Attachments = new List<Attachment>() { GetInlineAttachment() };

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (string.Equals(turnContext.Activity.Text, "uri", StringComparison.InvariantCultureIgnoreCase))
            {
                var reply = MessageFactory.Text("This is an Internet Attachment with file.");
                reply.Attachments = new List<Attachment>() { GetInternetAttachment() };

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (string.Equals(turnContext.Activity.Text, "upload", StringComparison.InvariantCultureIgnoreCase))
            {
                var reply = MessageFactory.Text("This is an Internet Attachment with upload.");
                var uploadedAttachment = await GetUploadedAttachmentAsync(turnContext, turnContext.Activity.ServiceUrl, turnContext.Activity.Conversation.Id, cancellationToken);
                reply.Attachments = new List<Attachment>() { uploadedAttachment };

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (string.Equals(turnContext.Activity.Text, "card", StringComparison.InvariantCultureIgnoreCase))
            {
                var card = GetHeroCard();
                var reply = MessageFactory.Attachment(card.ToAttachment());
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (string.Equals(turnContext.Activity.Text, "adaptive", StringComparison.InvariantCultureIgnoreCase))
            {
                var cardAttachment = CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json"));

                await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);

                await turnContext.SendActivityAsync(MessageFactory.Text("Please enter any text to see another card."), cancellationToken);
            }
            else if (string.Equals(turnContext.Activity.Text, "carousel", StringComparison.InvariantCultureIgnoreCase))
            {
                // Cards are sent as Attachments in the Bot Framework.
                // So we need to create a list of attachments for the reply activity.
                var attachments = new List<Attachment>();

                // Reply to the activity we received with an activity.
                var reply = MessageFactory.Attachment(attachments);
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments.Add(CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json")));
                reply.Attachments.Add(CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json")));
                reply.Attachments.Add(CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json")));

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                var text = turnContext.Activity.Text ?? (turnContext.Activity.Value as JObject)["PolicyNo"].Value<string>();
                var replyText = $"Echo: {text}. Say 'wait','image','upload','adaptive','carousel', to watch me type.";
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        private static Attachment GetInlineAttachment()
        {
            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.pdf");
            var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            return new Attachment
            {
                Name = @"architecture-resize.pdf",
                ContentType = "application/pdf",
                ContentUrl = $"data:application/pdf;base64,{imageData}",
            };
        }

        private static Attachment GetInternetAttachment()
        {
            // ContentUrl must be HTTPS.
            return new Attachment
            {
                Name = @"architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
            };
        }
        private static async Task<Attachment> GetUploadedAttachmentAsync(ITurnContext turnContext, string serviceUrl, string conversationId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources", "architecture-resize.png");

            var connector = turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;
            var attachments = new Attachments(connector);
            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                conversationId,
                new AttachmentData
                {
                    Name = @"Resources\architecture-resize.png",
                    OriginalBase64 = File.ReadAllBytes(imagePath),
                    Type = "image/png",
                },
                cancellationToken);

            var attachmentUri = attachments.GetAttachmentUri(response.Id);

            return new Attachment
            {
                Name = @"architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = attachmentUri,
            };
        }


        public static HeroCard GetHeroCard()
        {
            var card = new HeroCard
            {
                Title = "英雄卡",
                Subtitle = "關羽",
                Text = $@"
關羽（約160年－220年）[註 1]，字雲長，本字長生，司隸河東解良人（今山西省運城市），約生於東漢桓帝延熹年間[註 2]，
漢末三國時劉備的主要親信和將領。與張飛並稱「萬人敵」[1]。建安四年（199年），受封漢壽亭侯。.....。
",
                Images = new List<CardImage> { new CardImage("https://upload.wikimedia.org/wikipedia/commons/thumb/8/8b/God_Guan_Yu_Jinguashi_02.jpg/374px-God_Guan_Yu_Jinguashi_02.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "更多", value: "https://zh.wikipedia.org/wiki/%E5%85%B3%E7%BE%BD") },
            };


            //var card = new SigninCard
            //{
            //    Text = "BotFramework Sign-in Card",
            //    Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "https://login.microsoftonline.com/") },
            //};

            return card;
        }


        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

    }
}
