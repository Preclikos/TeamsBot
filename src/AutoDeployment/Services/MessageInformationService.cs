using AutoDeployment.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;

namespace AutoDeployment.Services
{
    public class MessageInformationService : IMessageInformationService
    {
        public ConversationInfo ConversationContext { get; }
        public MessageInformationService(IHttpContextAccessor httpContextAccessor)
        {
            var request = httpContextAccessor.HttpContext.Request;
            request.EnableBuffering();

            using (StreamReader stream = new StreamReader(request.Body, leaveOpen: true))
            {

                var bodyTask = stream.ReadToEndAsync();
                bodyTask.Wait();
                ConversationContext = JsonConvert.DeserializeObject<ConversationInfo>(bodyTask.Result);
            }

            request.Body.Seek(0, SeekOrigin.Begin);
        }
        public class ConversationInfo
        {
            [JsonProperty("channelId")]
            public string ChannelId { get; set; }
            [JsonProperty("from")]
            public Sender From { get; set; }
            [JsonProperty("conversation")]
            public Conversation Conversation { get; set; }

        }
        public class Conversation
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }
        public class Sender
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }
    }
}
