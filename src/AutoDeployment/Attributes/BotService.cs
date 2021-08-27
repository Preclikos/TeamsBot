using System;


namespace AutoDeployment.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BotService : Attribute
    {
        public string CommandName { get; set; }
        public bool PrivateChat { get; set; }
        public BotService(string commandName, bool privateChat = false)
        {
            CommandName = commandName;
            PrivateChat = privateChat;
        }
    }
}
