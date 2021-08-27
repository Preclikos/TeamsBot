using Newtonsoft.Json;

namespace AutoDeployment.BotServices
{
    public partial class BotReleaseService
    {
        public class PlayActionButton
        {
            [JsonProperty("command")]
            public string Command { get; set; }
            [JsonProperty("uniqueId")]
            public string UniqueId { get; set; }
            [JsonProperty("subCommand")]
            public string SubCommand { get; set; }
            [JsonProperty("time")]
            public long TimeStamp { get; set; }
            [JsonProperty("projectId")]
            public int ProjectId { get; set; }
            [JsonProperty("jobId")]
            public int JobId { get; set; }
        }

        public class CardData
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("mergeName")]
            public string MergeName { get; set; }

            [JsonProperty("mergeStateImage")]
            public string MergeStateImage { get; set; }

            [JsonProperty("changes")]
            public int Changes { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("approved")]
            public bool Approved { get; set; }

            [JsonProperty("approvedImageUrl")]
            public string ApprovedImageUrl { get; set; }

            [JsonProperty("approvedBy")]
            public string ApprovedBy { get; set; }

            [JsonProperty("manualJobPlay")]
            public PlayActionButton ManualJobPlay { get; set; } = new PlayActionButton();

            [JsonProperty("containsManualJob")]
            public bool ContainsManualJob { get; set; } = false;

            [JsonProperty("pipelines")]
            public JobImageUrl[] Pipelines { get; set; } = new JobImageUrl[0];
        }

        public class ButtonData
        {
            [JsonProperty("buttonTitle")]
            public string Title { get; set; }

            [JsonProperty("buttonData")]
            public ButtonClickData Data { get; set; } = new ButtonClickData();
        }
        public class ButtonClickData
        {
            [JsonProperty("command")]
            public string Command { get; set; }

            [JsonProperty("subCommand")]
            public string SubCommand { get; set; }

            [JsonProperty("uniqueId")]
            public string UniqueId { get; set; }

            [JsonProperty("time")]
            public long TimeStamp { get; set; }
        }
        public class JobImageUrl
        {
            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class GroupList
        {
            public GroupList(GroupInfo[] groups)
            {
                Groups = groups;
            }
            [JsonProperty("projectGroups")]
            public GroupInfo[] Groups { get; set; }
        }
        public class GroupInfo
        {
            public GroupInfo(string name, int groupId)
            {
                Name = name;
                GroupId = groupId;
            }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("value")]
            public int GroupId { get; set; }
        }
        public class CreateGroupReleaseData
        {
            [JsonProperty("command")]
            public string Command { get; set; }

            [JsonProperty("subCommand")]
            public string SubCommand { get; set; }

            [JsonProperty("uniqueId")]
            public string UniqueId { get; set; }
        }
        public class CreateGroupRelease
        {
            [JsonProperty("buttonData")]
            public CreateGroupReleaseData Data { get; set; } = new CreateGroupReleaseData();
            [JsonProperty("buttonTitle")]
            public string ButtonTitle { get; set; }
        }

        public class ResponseCreateGroupRelease
        {
            [JsonProperty("command")]
            public string Command { get; set; }

            [JsonProperty("subCommand")]
            public string SubCommand { get; set; }

            [JsonProperty("uniqueId")]
            public string UniqueId { get; set; }

            [JsonProperty("selectedGroup")]
            public int SelectedGroup { get; set; }
        }

        public class CreateGroupReleaseVersion
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("group")]
            public string GroupName { get; set; }
        }
    }
}
