using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using kumaS.NuGetImporter.Editor.DataClasses;

using UnityEngine;

namespace kumaS.NuGetImporter.Editor
{
    internal sealed class PackageControllerAsUPM : PackageControllerBase
    {
        public PackageControllerAsUPM()
        {
            PathSolver = new UPMPathSolver();
        }

        /// <inheritdoc/>
        internal override void DeletePluginsOutOfDirectory(Package package) { }

        /// <inheritdoc/>
        internal override async Task<(bool isSkipped, Package package, PackageManagedPluginList asm)>
            InstallPackageAsync(Package package, IEnumerable<string> loadedAsmName)
        {
            Task<string> task = PathSolver.InstallPath(package);
            Task<Catalog> task2 = NuGet.GetCatalog(package.ID);
            await ExtractPackageAsync(package);
            var installPath = await task;
            var asm = new PackageManagedPluginList { packageName = package.ID, fileNames = new List<string>() };
            GetLoadableAsmInPackage(installPath, asm);
            if (asm.fileNames.Intersect(loadedAsmName).Any())
            {
                DeleteDirectory(installPath);
                return (true, package, asm);
            }

            Catalog catalog = await task2;
            Catalogentry selectedCatalog =
                catalog.GetAllCatalogEntry().First(entry => entry.version == package.Version);
            File.WriteAllText(
                Path.Combine(installPath, "package.json"),
                JsonUtility.ToJson(selectedCatalog.ToPackageJson(), true)
            );
            return (false, package, asm);
        }
    }
}
