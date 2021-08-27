using AutoDeployment.Interfaces;
using AutoDeployment.Models.GitLab;
using System.Collections.Generic;
using System.Linq;

namespace AutoDeployment.Services
{
    public class ConversationObjectStorage : IUserObjectStorage
    {
        IObjectStorage ObjectStorageService { get; set; }
        string ConversationId { get; set; }
        public ConversationObjectStorage(IObjectStorage objectStorage, IMessageInformationService messageInformation)
        {
            ObjectStorageService = objectStorage;
            ConversationId = messageInformation.ConversationContext.Conversation.Id;

            if (!ObjectStorageService.ReleaseMerges.Any(a => a.Key == ConversationId))
            {
                objectStorage.ReleaseMerges.Add(ConversationId, new ListGitProjectReleaseMerge());
            }

            if (!ObjectStorageService.ProjectVersions.Any())
            {
                ObjectStorageService.ProjectVersions.Add(ConversationId, new List<ProjectTagVersion>());
            }
        }
        //Tracking models
        public List<GroupProject> GitLabGroupProjects { get { return ObjectStorageService.GitLabGroupProjects; } }
        public ListGitProjectReleaseMerge GitLabReleaseMerges
        {
            get
            {
                return ObjectStorageService.ReleaseMerges.Single(w => w.Key == ConversationId).Value;
            }
            set
            {
                ObjectStorageService.ReleaseMerges.Remove(ConversationId);
                ObjectStorageService.ReleaseMerges.Add(ConversationId, value);
            }
        }
public List<ProjectTagVersion> GitLabProjectVersions { 
            get { 
                return ObjectStorageService.ProjectVersions.Single(w => w.Key == ConversationId).Value; 
            } 
            set
            {
                ObjectStorageService.ProjectVersions.Remove(ConversationId);
                ObjectStorageService.ProjectVersions.Add(ConversationId, value);
            }
        }
    }
}
