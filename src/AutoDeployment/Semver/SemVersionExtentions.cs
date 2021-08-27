namespace AutoDeployment.Semver
{
    public static class SemVersionExtensions
    {
        public static SemVersion IncrementMajor(this SemVersion version) => new SemVersion(version.Major + 1);

        public static SemVersion IncrementMinor(this SemVersion version) => new SemVersion(version.Major, version.Minor + 1);

        public static SemVersion IncrementPatch(this SemVersion version) => new SemVersion(version.Major, version.Minor, version.Patch + 1);
    }
}
