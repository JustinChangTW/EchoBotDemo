using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBotDemo
{
    public class ConversationData
    {
        public string Timestamp { get; set; }
        public string ChannelId { get; set; }
        public bool PromptedUserForName { get; set; } = false;
        public bool PromptedUserForAge { get; set; } = false;

        public Question LastQuestionAsked { get; set; } = Question.None;

    }

    public enum Question
    {
        Name,
        Age,
        Date,
        None,
    }
}
