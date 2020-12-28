using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace EchoBotDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly string _appId;

        public NotifyController(IBotFrameworkHttpAdapter botFrameworkHttpAdapter,
            IConfiguration configuration, 
            ConcurrentDictionary<string,ConversationReference> conversationReference)
        {
            _adapter = botFrameworkHttpAdapter;
            _configuration = configuration;
            _conversationReferences = conversationReference;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        public async Task<ActionResult> Get(string message)
        {
            
            
            foreach(var conversationReference in _conversationReferences.Values)
            {
                try
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference,
                        async (turnContext, cancellationToken) => await turnContext.SendActivityAsync(message),
                        default(CancellationToken));
                }
                catch
                {
                    Console.WriteLine("使用者已離線");
                }

            }

            return Ok();
        }


    }
}
