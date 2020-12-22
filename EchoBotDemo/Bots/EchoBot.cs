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
using Microsoft.Bot.Schema;

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
            else
            {
                var replyText = $"Echo: {turnContext.Activity.Text}. Say 'wait','image' to watch me type.";
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
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png",
            };
        }

    }
}
