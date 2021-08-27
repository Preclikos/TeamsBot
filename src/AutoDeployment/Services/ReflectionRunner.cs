using AutoDeployment.Interfaces;
using AutoDeployment.Models.CommandModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.Services
{

    public class ReflectionRunner : IReflectionRunner
    {
        private IServiceProvider ServiceProvider { get; set; }
        ILogger<ReflectionRunner> Logger { get; set; }
        ICommandService CommandList { get; set; }
        public ReflectionRunner(IServiceProvider serviceProvider, ILogger<ReflectionRunner> logger, ICommandService commandList)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            CommandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
        }

        private (SubCommand CommandInfo, Object ClassInstance) GetCommandFromList(string command, string subCommand)
        {
            var commandClass = CommandList.Commands.Single(s => s.Name == command);
            var commandInstance = ServiceProvider.GetService(commandClass.CommandClass);

            var subCommandClass = commandClass.SubCommands.Single(s => s.Name == subCommand);
            return (CommandInfo: subCommandClass, ClassInstance: commandInstance);
        }

        public async Task RunCommand(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string command, string subCommand, string uniqueMessageId = null, string[] textCommandAttributes = null)
        {
            var commandClasses = GetCommandFromList(command, subCommand);

            _ = (Task)(commandClasses.CommandInfo.SubCommandMethod).Invoke(
                commandClasses.ClassInstance, new object[] { turnContext, cancellationToken, uniqueMessageId, textCommandAttributes }
                );
        }

        public async Task RunCommand(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string command, string subCommand, string uniqueMessageId = null, JObject textCommandAttributes = null)
        {
            var commandClasses = GetCommandFromList(command, subCommand);

            var subCommandParameters = commandClasses.CommandInfo.SubCommandMethod.GetParameters().Single(s => s.Name == "textCommandAttributes");

            _ = (Task)(commandClasses.CommandInfo.SubCommandMethod).Invoke(
                commandClasses.ClassInstance, new object[] { turnContext, cancellationToken, uniqueMessageId, ParseObjectToClass(subCommandParameters.ParameterType, textCommandAttributes) }
                );

        }

        private object ParseObjectToClass(Type type,JObject values)
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize(new JTokenReader(values), type);
        }
    }
}
