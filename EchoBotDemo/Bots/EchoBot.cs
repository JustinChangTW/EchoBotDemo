// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EchoBotDemo.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IStorage _storage;

        public EchoBot(ConversationState conversationState, UserState userState, 
            ConcurrentDictionary<string,ConversationReference> conversationReferences, IStorage storage)
        {
            _conversationState = conversationState;
            _userState = userState;
            _conversationReferences = conversationReferences;
            _storage = storage;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //取得記錄的會話狀態
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            //取得使用者輸入的資料
            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());


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

                var reply = MessageFactory.Attachment(new List<Attachment>());
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments.Add(CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json")));
                reply.Attachments.Add(CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json")));
                reply.Attachments.Add(CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "adaptiveCard.json")));
                reply.Attachments.Add(GetHeroCard().ToAttachment());

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else if (conversationData.LastQuestionAsked!= Question.None ||  string.Equals(turnContext.Activity.Text, "username", StringComparison.InvariantCultureIgnoreCase))
            {
                string replyText = "";
                replyText = FillOutUserProfileAsync(turnContext, conversationData, userProfile, replyText);
                var reply = MessageFactory.Text(replyText);
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                var text = turnContext.Activity.Text ?? (turnContext.Activity.Value as JObject)["PolicyNo"].Value<string>();
                
                var replyText = $"Echo: {text}. Say 'wait','image','upload','adaptive','carousel', to watch me type.";
                var reply = MessageFactory.Text(replyText, replyText);
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                    {
                        new CardAction() { Title = "請輸入使用者姓名", Type = ActionTypes.ImBack, Value = "username", Image = "https://via.placeholder.com/20/FF0000?text=R", ImageAltText = "R" },
                        new CardAction() { Title = "wait", Type = ActionTypes.ImBack, Value = "wait", Image = "https://via.placeholder.com/20/FF0000?text=R", ImageAltText = "R" },
                        new CardAction() { Title = "image", Type = ActionTypes.ImBack, Value = "image", Image = "https://via.placeholder.com/20/FFFF00?text=Y", ImageAltText = "Y" },
                        new CardAction() { Title = "upload", Type = ActionTypes.ImBack, Value = "upload", Image = "https://via.placeholder.com/20/0000FF?text=B", ImageAltText = "B"   },
                        new CardAction() { Title = "adaptive", Type = ActionTypes.ImBack, Value = "adaptive", Image = "https://via.placeholder.com/20/FFFFFF?text=B", ImageAltText = "B"   },
                        new CardAction() { Title = "carousel", Type = ActionTypes.ImBack, Value = "carousel", Image = "https://via.placeholder.com/20/008000?text=B", ImageAltText = "Green"   },
                    },
                };

                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
        }

        private static string FillOutUserProfileAsync(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, UserProfile userProfile, string replyText)
        {
            var text = turnContext.Activity.Text?.Trim();
            var message = "";
            switch (conversationData.LastQuestionAsked)
            {
                case Question.None:
                    userProfile.Name = text;
                    conversationData.LastQuestionAsked = Question.Name;
                    replyText = $"請輸入您的姓名？";
                    break;
                case Question.Name:
                    if(ValidateName(text,out var name,out message))
                    {
                        userProfile.Name = name;
                        conversationData.LastQuestionAsked = Question.Age;
                        replyText = $"您的姓名是:{userProfile.Name}，請輸入您的年齡？";
                    }
                    else
                    {
                        replyText = message ?? $"對不起，我不明白。";
                    }

                    break;
                case Question.Age:
                    if(ValidateAge(text, out var age, out message))
                    {
                        userProfile.Age = age;
                        conversationData.LastQuestionAsked = Question.None;

                        replyText = @$"
您的姓名是:{userProfile.Name}，
您的年齡是{userProfile.Age}";
                    }
                    else
                    {
                        replyText = message ?? $"對不起，我不明白。";
                    }

                    break;
            }

            return replyText;
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

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
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
                Text = @"  
關羽（約160年－220年）[註 1]，字雲長，本字長生，司隸河東解良人（今山西省運城市），約生於東漢桓帝延熹年間[註2]，
漢末三國時劉備的主要親信和將領。與張飛並稱「萬人敵」[1]。建安四年（199年），受封漢壽亭侯。.....。
",
                Images = new List<CardImage> { new CardImage("https://upload.wikimedia.org/wikipedia/commons/thumb/8/8b/God_Guan_Yu_Jinguashi_02.jpg/374px-God_Guan_Yu_Jinguashi_02.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "更多", value: "https://zh.wikipedia.org/wiki/%E5%85%B3%E7%BE%BD") }
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



        private static bool ValidateName(string input, out string name, out string message)
        {
            name = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "請輸入至少包含一個字符的名稱。";
            }
            else
            {
                name = input.Trim();
            }

            return message is null;
        }

        private static bool ValidateAge(string input, out int age, out string message)
        {
            age = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.Chinese);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        age = Convert.ToInt32(value);
                        if (age >= 18 && age <= 120)
                        {
                            return true;
                        }
                    }
                }

                message = "請輸入18至120之間的年齡。";
            }
            catch
            {
                message = "抱歉，我無法將其解釋為年齡。請輸入18至120之間的年齡。";
            }

            return message is null;
        }



        private void AddConversationReference(Activity activity, CancellationToken cancellationToken)
        {


            var conversationReference = activity.GetConversationReference();

            //從storage取得資料
            var notifiesStorage = new List<Notify>();
            try
            {
                notifiesStorage = _storage.ReadAsync<List<Notify>>(new string[] { "Notify" }).Result?.Values.FirstOrDefault();
                notifiesStorage = notifiesStorage ?? new List<Notify>();
            }
            catch
            {
                notifiesStorage = notifiesStorage ?? new List<Notify>();
            }

            //從storage同步到memory
            if (notifiesStorage != null)
            {
                foreach (var item in notifiesStorage.Distinct())
                {
                    _conversationReferences.AddOrUpdate(item.Id, conversationReference, (key, newValue) => conversationReference);
                }
            }

            //刪除登出的使用者
            if (activity.MembersRemoved != null)
            {
                foreach (var item in activity.MembersRemoved)
                {
                    ConversationReference reference;

                    _conversationReferences.TryRemove(item.Id, out reference);
                }
            }

            //加入新的使用者
            var changes = new Dictionary<string, object>();
            if (activity.MembersAdded != null)
            {
                foreach (var item in activity.MembersAdded)
                {
                    _conversationReferences.AddOrUpdate(item.Id, conversationReference, (key, newValue) => conversationReference);

                    notifiesStorage.Add(new Notify { Id = item.Id });
                }
            }

            changes.Add("Notify", notifiesStorage.Distinct().ToList());
            _storage.WriteAsync(changes, cancellationToken);

            //_conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity, cancellationToken);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }
    }
    class Notify
    {
        public string Id { get; set; }
    }
}
