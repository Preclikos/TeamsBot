using System.Collections.Generic;

namespace AutoDeployment.Enums
{
    public static class Channel
    {
        private static readonly Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();

        static Channel()
        {
            keyValuePairs.Add((int)Name.Test_JHS, "19:57cddd0c4244478492273dc99e29a0da@thread.skype");
            keyValuePairs.Add((int)Name.Releases, "19:903124fcbaf2418c8e237238f022c875@thread.skype");
        }

        public enum Name { Test_JHS = 1, Releases = 2, Blue = 4, Yellow = 8 };

        public static string GetId(Name names)
        {
            return keyValuePairs.GetValueOrDefault((int)names, null);
        }
    }
}
