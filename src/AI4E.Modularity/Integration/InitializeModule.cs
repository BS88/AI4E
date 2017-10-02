namespace AI4E.Modularity.Integration
{
    public sealed class InitializeModule { }

    public sealed class ModuleInitialized
    {
        public ModuleInitialized(ModuleIdentifier module, ModuleVersion version)
        {
            Module = module;
            Version = version;
        }

        public ModuleIdentifier Module { get; }
        public ModuleVersion Version { get; }
    }
}
