using AdaptiveCards;
using AdaptiveCards.Templating;
using AutoDeployment.Models.GitLab;
using GitLabApiClient.Models.Job.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.BotServices
{
    public partial class BotReleaseService
    {
        public async Task UpdateReleaseGroupActivity(ITurnContext turnContext, string uniqueId, List<(string Name, int GroupId)> groups, bool singleProject,CancellationToken cancellationToken)
        {
            var releaseCard = CreateReleaseGroupCardFromJson(uniqueId, "Select group for new Release: ", groups, singleProject);
            var releaseActivity = CardHelpers.ConvertToActivity(releaseCard, uniqueId);
            await CardHelpers.UpdateOrSendActivity(turnContext, releaseActivity, false, cancellationToken);
        }
        public async Task UpdateReleaseGroupActivity(ITurnContext turnContext, string uniqueId, string groupName, string version, CancellationToken cancellationToken)
        {
            var releaseCard = CreateReleaseGroupCardFromJson(uniqueId, groupName, version);
            var releaseActivity = CardHelpers.ConvertToActivity(releaseCard, uniqueId);
            await CardHelpers.UpdateOrSendActivity(turnContext, releaseActivity, false, cancellationToken);
        }

        public async Task UpdateReleaseGroupActivity(ITurnContext turnContext, string uniqueId, IEnumerable<(string Name, int Id)> projects,CancellationToken cancellationToken)
        {
            var releaseCard = CreateReleaseGroupCardFromJson(uniqueId, "Select group with project for Release: ", projects.ToList(), false);
            var releaseActivity = CardHelpers.ConvertToActivity(releaseCard, uniqueId);
            await CardHelpers.UpdateOrSendActivity(turnContext, releaseActivity, false, cancellationToken);
        }

        public static async Task UpdateReleaseActivity(ITurnContext turnContext, IEnumerable<GitProjectReleaseMerge> projectReleaseMerges, string uniqueId, long timeStamp, CancellationToken cancellationToken)
        {
            var releaseCard = CreateReleaseCardFromJson(projectReleaseMerges, uniqueId, timeStamp);
            var releaseActivity = CardHelpers.ConvertToActivity(releaseCard, uniqueId);
            await CardHelpers.UpdateOrSendActivity(turnContext, releaseActivity, false, cancellationToken);
        }
        public async Task UpdateReleaseActivity(ITurnContext<IMessageActivity> turnContext, IEnumerable<GitProjectReleaseMerge> projectReleaseMerges, string uniqueId, long timeStamp, CancellationToken cancellationToken)
        {
            var releaseCard = CreateReleaseCardFromJson(projectReleaseMerges, uniqueId, timeStamp);
            var releaseActivity = CardHelpers.ConvertToActivity(releaseCard, uniqueId);
            await CardHelpers.UpdateOrSendActivity(turnContext, releaseActivity, false, cancellationToken);
        }

        public static AdaptiveCard CreateReleaseGroupCardFromJson(string uniqueId, string cardText, List<(string Name, int GroupId)> groups, bool singleProject, bool singleProjectCreate = false)
        {

            var groupListData = groups.Select(s => new GroupInfo(s.Name, s.GroupId)).ToArray();
            var groupList = new GroupList(groupListData);

            string groupTemplate = File.ReadAllText("BotServices/ReleaseTemplates/ReleaseGroupCreate.json");
            string dataJson = JsonConvert.SerializeObject(groupList);
            var cardItemJson = CardHelpers.TransformTemplateToData(groupTemplate, dataJson);

            string buttonJson = CreateGroupReleaseButton(uniqueId, "Create", singleProject, singleProjectCreate);

            return CardHelpers.ConvertToAdaptiveCard(new List<string>() { cardItemJson }, new List<string>() { buttonJson });
        }
        
        public static AdaptiveCard CreateReleaseGroupCardFromJson(string uniqueId, string groupName, string version)
        {
            var versionData = new CreateGroupReleaseVersion() { Version = version, GroupName = groupName };
            string dataJson = JsonConvert.SerializeObject(versionData);
            string groupTemplate = File.ReadAllText("BotServices/ReleaseTemplates/ReleaseGroupVersion.json");
            var cardItemJson = CardHelpers.TransformTemplateToData(groupTemplate, dataJson);

            return CardHelpers.ConvertToAdaptiveCard(new List<string>() { cardItemJson }, new List<string>());
        }

        public static AdaptiveCard CreateReleaseCardFromJson(IEnumerable<GitProjectReleaseMerge> projectReleaseMerge, string uniqueId, long dateTimeTicks)
        {

            string headerTemplate = File.ReadAllText("BotServices/ReleaseTemplates/ReleaseHeader.json");

            var cardState = GetStateByMergeRequests(projectReleaseMerge);

            string selectedSubSection = File.ReadAllText(GetJsonResourceByState(cardState));

            List<String> bodyReleaseElements = new List<string>();

            foreach (var release in projectReleaseMerge)
            {
                var releaseData = new CardData()
                {
                    Name = release.GitProject.Name,
                    MergeName = release.GitMergeRequest.Title,
                    MergeStateImage = MergeStateImage(release.GitMergeRequest.State),
                    Status = release.GitMergeRequest.Status.ToString(),
                    Changes = release.GitMergeRequest.ChangesCount ?? 0
                };

                if (release.GitMergeApprovalState.Rules.Any() &&
                    release.GitMergeApprovalState.Rules.FirstOrDefault().ApprovedBy.Any())
                {
                    releaseData.Approved = true;
                    releaseData.ApprovedImageUrl = release.GitMergeApprovalState.Rules.First().ApprovedBy.First().AvatarUrl;
                    releaseData.ApprovedBy = release.GitMergeApprovalState.Rules.First().ApprovedBy.First().Name;
                }
                else
                {
                    releaseData.Approved = false;
                    releaseData.ApprovedBy = "No approval required!";
                }

                if (release.GitPipelineJobs != null && release.GitPipelineJobs.Any())
                {
                    foreach (var job in release.GitPipelineJobs)
                    {
                        var tempList = releaseData.Pipelines.ToList();
                        var jobImage = GetImageByState(GetStateByPriority(job));
                        tempList.Add(new JobImageUrl() { Value = jobImage });
                        releaseData.Pipelines = tempList.ToArray();


                    }
                    if (release.GitPipelineJobs.Any(c => c.Any(a => a.Status == "manual")))
                    {
                        var stageWithManual = release.GitPipelineJobs.First(c => c.Any(a => a.Status == "manual"));
                        var manualStep = stageWithManual.First(s => s.Status == "manual");
                        releaseData.ContainsManualJob = true;

                        releaseData.ManualJobPlay.Command = "release";
                        releaseData.ManualJobPlay.SubCommand = "jobplay";
                        releaseData.ManualJobPlay.UniqueId = uniqueId;
                        releaseData.ManualJobPlay.TimeStamp = dateTimeTicks;
                        releaseData.ManualJobPlay.ProjectId = release.GitProject.Id;
                        releaseData.ManualJobPlay.JobId = manualStep.Id;
                    }
                }

                string dataJson = JsonConvert.SerializeObject(releaseData);
                string fullTemplate = headerTemplate.Replace("{SubSection}", selectedSubSection);
                var cardItemJson = CardHelpers.TransformTemplateToData(fullTemplate, dataJson);

                bodyReleaseElements.Add(cardItemJson);

            }

            var actionReleaseElements = new List<string>();
            if (cardState != ReleaseState.Tags && cardState != ReleaseState.Closed)
            {
                actionReleaseElements.Add(CreateButtonByState(cardState, uniqueId, dateTimeTicks));
            }
            return CardHelpers.ConvertToAdaptiveCard(bodyReleaseElements, actionReleaseElements);
        }

        /* failed
        warning
        pending
        running
        manual
        scheduled
        canceled
        success
        skipped
        created*/
        private static JobState GetStateByPriority(IGrouping<string, Job> jobs)
        {
            var statusList = jobs.Select(s => s.Status);
            if (statusList.Contains("manual"))
            {
                return JobState.Manual;
            }
            if (statusList.Contains("running"))
            {
                return JobState.Running;
            }
            if (statusList.Contains("pending"))
            {
                return JobState.Pending;
            }
            if (statusList.Contains("failed"))
            {
                return JobState.Failed;
            }
            if (statusList.Contains("success"))
            {
                return JobState.Success;
            }
            if (statusList.Contains("skipped"))
            {
                return JobState.Skipped;
            }
            return JobState.Created;
        }

        private static string GetImageByState(JobState state)
        {
            switch (state)
            {
                case JobState.Manual:
                    return "https://financebot.preclikos.cz:4567/states/manual.png";
                case JobState.Failed:
                    return "https://financebot.preclikos.cz:4567/states/failed.png";
                case JobState.Pending:
                    return "https://financebot.preclikos.cz:4567/states/pending.png";
                case JobState.Success:
                    return "https://financebot.preclikos.cz:4567/states/success.png";
                case JobState.Running:
                    return "https://financebot.preclikos.cz:4567/states/running.png";
                default:
                    return "https://financebot.preclikos.cz:4567/states/created.png";
            }
        }

        private static string CreateGroupReleaseButton(string uniqueId, string Title, bool singleProject, bool singleProjectCreate = false)
        {
            var transformer = new AdaptiveTransformer();

            var buttonData = new CreateGroupRelease();

            buttonData.Data.Command = "release";
            buttonData.Data.SubCommand = "groupcreate";
            if(singleProject)
            {
                buttonData.Data.SubCommand = "listgroupproject";
                if (singleProjectCreate)
                {
                    buttonData.Data.SubCommand = "createprojectrelease";
                }
            }
            buttonData.Data.UniqueId = uniqueId;
            buttonData.ButtonTitle = Title;


            string submitButton = @"
            {
                ""type"": ""Action.Submit"",
                ""title"": ""{buttonTitle}"",
                ""data"": ""{buttonData}""
            }";

            string dataJson = JsonConvert.SerializeObject(buttonData);
            return transformer.Transform(submitButton, dataJson);
        }

        private static string CreateButtonByState(ReleaseState releaseState, string uniqueId, long dateTimeTicks)
        {
            var transformer = new AdaptiveTransformer();

            var buttonData = new ButtonData();

            buttonData.Data.Command = "release";
            buttonData.Data.UniqueId = uniqueId;
            buttonData.Data.TimeStamp = dateTimeTicks;

            switch (releaseState)
            {
                case ReleaseState.Clean:
                    buttonData.Title = "Clean";
                    buttonData.Data.SubCommand = "clean";
                    break;
                case ReleaseState.Approve:
                    buttonData.Title = "Approve";
                    buttonData.Data.SubCommand = "approve";
                    break;

                case ReleaseState.Merge:
                    buttonData.Title = "Merge";
                    buttonData.Data.SubCommand = "merge";
                    break;
            }

            string submitButton = @"
            {
                ""type"": ""Action.Submit"",
                ""title"": ""{buttonTitle}"",
                ""data"": ""{buttonData}""
            }";

            string dataJson = JsonConvert.SerializeObject(buttonData);
            return transformer.Transform(submitButton, dataJson);
        }

        private static string GetJsonResourceByState(ReleaseState releaseState)
        {
            switch (releaseState)
            {
                case ReleaseState.Merge:
                    return "BotServices/ReleaseTemplates/ReleaseApproved.json";
                case ReleaseState.Tags:
                    return "BotServices/ReleaseTemplates/ReleasePipeline.json";
                case ReleaseState.Clean:
                case ReleaseState.Approve:
                case ReleaseState.Closed:
                    return "BotServices/ReleaseTemplates/ReleaseChanges.json";
                default:
                    return "BotServices/ReleaseTemplates/ReleaseChanges.json";
            }

        }

        private static ReleaseState GetStateByMergeRequests(IEnumerable<GitProjectReleaseMerge> releaseMerges)
        {
            if(releaseMerges.All(a => a.GitMergeRequest.State == MergeRequestState.Closed))
            {
                return ReleaseState.Closed;
            }
            var invalidMRCount = releaseMerges.Where(c =>
                c.GitMergeRequest.Status == MergeStatus.CannotBeMerged &&
                c.GitMergeRequest.ChangesCount == null && c.GitMergeRequest.State != MergeRequestState.Closed);
            if (invalidMRCount.Count() > 0)
            {
                return ReleaseState.Clean;
            }
            else if (releaseMerges.Any(a => a.GitMergeRequest.State == MergeRequestState.Opened && a.ApprovalState() == false))
            {
                return ReleaseState.Approve;
            }
            else if (releaseMerges.Any(w => w.GitMergeRequest.State == MergeRequestState.Opened && w.ApprovalState() == true))
            {
                return ReleaseState.Merge;
            }
            else if (releaseMerges.All(a => a.GitMergeRequest.State == MergeRequestState.Merged || a.GitMergeRequest.State == MergeRequestState.Closed))
            {
                return ReleaseState.Tags;
            }
            return ReleaseState.Error;
        }

        enum ReleaseState
        {
            Error = 0,
            Clean = 1,
            Approve = 2,
            Merge = 3,
            Tags = 4,
            Closed = 5
        };

        public enum JobState
        {
            Failed,
            Warning,
            Pending,
            Running,
            Manual,
            Scheduled,
            Canceled,
            Success,
            Skipped,
            Created
        };

        private static string MergeStateImage(MergeRequestState requestState)
        {
            switch (requestState)
            {
                case MergeRequestState.Opened:
                    return "https://financebot.preclikos.cz:4567/open.png";
                case MergeRequestState.Closed:
                    return "https://financebot.preclikos.cz:4567/closed.png";
                case MergeRequestState.Merged:
                    return "https://financebot.preclikos.cz:4567/merged.png";
            }
            return "";
        }
    }
}
