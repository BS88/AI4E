namespace AI4E.Modularity
{
    public sealed partial class ModuleInstaller
    {
        private sealed class ModuleInstallation : IModuleInstallation
        {
            public ModuleInstallation(ModuleIdentifier module, IModuleMetadata metadata, string installationDirectory, IModuleSource source)
            {
                Module = module;
                ModuleMetadata = metadata;
                Source = source;
                InstallationDirectory = installationDirectory;
            }

            public IModuleSource Source { get; }

            public ModuleIdentifier Module { get; }

            public ModuleVersion Version => ModuleMetadata?.Version ?? ModuleVersion.Unknown;

            public IModuleMetadata ModuleMetadata { get; }

            public string InstallationDirectory { get; }
        }
    }
}
