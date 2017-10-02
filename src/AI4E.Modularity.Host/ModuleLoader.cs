﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AI4E.Async;
using Newtonsoft.Json;

namespace AI4E.Modularity
{
    public sealed partial class ModuleLoader : IModuleLoader
    {
        private readonly IModuleSource _source;

        public ModuleLoader(IModuleSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            _source = source;
        }

        public async Task<IEnumerable<ModuleReleaseIdentifier>> ListModulesAsync()
        {
            var directory = new DirectoryInfo(_source.Source);

            if (!directory.Exists)
                return Enumerable.Empty<ModuleReleaseIdentifier>();

            var files = directory.GetFiles("*.aep", SearchOption.AllDirectories);
            var result = new List<ModuleMetadata>();

            foreach (var file in files)
            {
                var metadata = await ReadMetadataAsync(file);

                if (metadata != null)
                {
                    result.Add(metadata);
                }
            }

            return result.Select(p => new ModuleReleaseIdentifier(new ModuleIdentifier(p.Name), p.Version));
        }

        public async Task<IModuleMetadata> LoadModuleMetadataAsync(ModuleReleaseIdentifier identifier)
        {
            var directory = new DirectoryInfo(_source.Source);

            if (!directory.Exists)
                return null;

            var hints = directory.GetFiles($"{identifier}.aep", SearchOption.AllDirectories);

            foreach (var hint in hints)
            {
                var metadata = await ReadMetadataAsync(hint);

                if (metadata != null && new ModuleReleaseIdentifier(new ModuleIdentifier(metadata.Name), metadata.Version) == identifier)
                {
                    return metadata;
                }
            }

            var files = directory.GetFiles("*.aep", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var metadata = await ReadMetadataAsync(file);

                if (metadata != null && new ModuleReleaseIdentifier(new ModuleIdentifier(metadata.Name), metadata.Version) == identifier)
                {
                    return metadata;
                }
            }

            return null;
        }

        private async Task<ModuleMetadata> ReadMetadataAsync(FileInfo file)
        {
            if (!file.Exists)
                return null;

            using (var fileStream = file.OpenReadAsync())
            using (var package = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                var manifest = package.GetEntry("module.json");

                // Invalid package
                if (manifest == null)
                {
                    return null;
                }

                using (var memoryStream = new MemoryStream())
                using (var manifestStream = manifest.Open())
                using (var manifestReader = new JsonTextReader(new StreamReader(memoryStream)))
                {
                    await manifestStream.CopyToAsync(memoryStream, 4096);
                    memoryStream.Position = 0;

                    var moduleManifest = JsonSerializer.Create().Deserialize<ModuleMetadata>(manifestReader);

                    // Invalid package
                    if (moduleManifest == null)
                    {
                        return null;
                    }

                    return moduleManifest;
                }
            }
        }

        public async Task<(Stream, IModuleMetadata)> LoadModuleAsync(ModuleReleaseIdentifier identifier)
        {
            var directory = new DirectoryInfo(_source.Source);

            if (!directory.Exists)
                return default;

            var hints = directory.GetFiles($"{identifier}.aep", SearchOption.AllDirectories);

            foreach (var hint in hints)
            {
                var metadata = await ReadMetadataAsync(hint);

                if (metadata != null && new ModuleReleaseIdentifier(new ModuleIdentifier(metadata.Name), metadata.Version) == identifier)
                {
                    return (hint.OpenReadAsync(), metadata);
                }
            }

            var files = directory.GetFiles("*.aep", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var metadata = await ReadMetadataAsync(file);

                if (metadata != null && new ModuleReleaseIdentifier(new ModuleIdentifier(metadata.Name), metadata.Version) == identifier)
                {
                    return (file.OpenReadAsync(), metadata);
                }
            }

            return default;
        }
    }
}
