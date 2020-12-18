using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

using static SimpleExec.Command;

using Console = Colorful.Console;

namespace Targets
{
    internal class DotnetSdkManager
    {
        private const string CUSTOM_SDK_INSTALL_DIR = ".dotnet";
        private const string BUILD_SUPPORT_DIR = ".build";
        private string dotnetPath;

        public string GetDotnetCliPath()
        {
            if (dotnetPath == null)
            {
                var (customSdk, sdkPath, sdkVersion) = EnsureRequiredSdkIsInstalled();
                Console.WriteLine($"Build will be executed using {(customSdk ? "user defined SDK" : "default SDK")}, Version '{sdkVersion}'.{(customSdk ? $" Installed at '{sdkPath}'" : string.Empty)}");
                dotnetPath = customSdk
                    ? Path.Combine(sdkPath, "dotnet")
                    : "dotnet";
            }

            return dotnetPath;
        }

        private (bool customSdk, string sdkPath, string sdkVersion) EnsureRequiredSdkIsInstalled()
        {
            var currentSdkVersion = Read("dotnet", "--version").TrimEnd(Environment.NewLine.ToCharArray());
            var requiredSdkFile = Directory.EnumerateFiles(".", ".required-sdk", SearchOption.TopDirectoryOnly).SingleOrDefault();

            if (string.IsNullOrWhiteSpace(requiredSdkFile))
            {
                Console.WriteLine("No custom SDK is required.", Color.Green);
                return (false, string.Empty, currentSdkVersion);
            }

            var requiredSdkVersion = File.ReadAllText(requiredSdkFile).TrimEnd(Environment.NewLine.ToCharArray());

            if (string.Compare(currentSdkVersion, requiredSdkVersion) == 0)
            {
                Console.WriteLine("Insalled SDK is the same as required one, '.required-sdk' file is not necessary. Build will use the SDK available on the machine.", Color.Yellow);
                return (false, string.Empty, currentSdkVersion);
            }

            Console.WriteLine($"Installed SDK ({currentSdkVersion}) doesn't match required one ({requiredSdkVersion}).", Color.Yellow);
            Console.WriteLine($"{requiredSdkVersion} will be installed and and used to run the build.", Color.Yellow);

            var installScriptName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "dotnet-install.ps1"
                : "dotnet-install.sh";

            var installScriptUrl = $"https://dot.net/v1/{installScriptName}";

            Console.WriteLine($"Downloading {installScriptName} script, from {installScriptUrl}.");

            Directory.CreateDirectory(BUILD_SUPPORT_DIR);
            new WebClient().DownloadFile(installScriptUrl, Path.Combine(BUILD_SUPPORT_DIR, installScriptName));

            Console.WriteLine($"Ready to install custom SDK, version {requiredSdkVersion}.");

            var installScriptLocation = Path.Combine(".", BUILD_SUPPORT_DIR, installScriptName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Run("powershell", $"{installScriptLocation} -Version {requiredSdkVersion} -InstallDir {CUSTOM_SDK_INSTALL_DIR}");
            else
                Run("bash", $"{installScriptLocation} --version {requiredSdkVersion} --install-dir {CUSTOM_SDK_INSTALL_DIR}");

            return (true, CUSTOM_SDK_INSTALL_DIR, requiredSdkVersion);
        }
    }
}