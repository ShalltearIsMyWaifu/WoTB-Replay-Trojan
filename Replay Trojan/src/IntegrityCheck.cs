using TrojanCompiler;
using TrojanPayload;

namespace Replay_Trojan
{
    static internal class IntegrityCheck
    {
        /// <summary>
        /// Verifies project integrity
        /// </summary>
        /// <returns>Original replay name and root project directory</returns>
        public static (string, DirectoryInfo) ProjectIntegrityCheck()
        {
            string originalReplayName = Task.Run(GetReplayName).GetAwaiter().GetResult(); // forcefully making async into synchronous

            DirectoryInfo rootProjectDirectory = VerifyProjectTree()!;

            IconFileValidation(rootProjectDirectory);

            return (originalReplayName, rootProjectDirectory);
        }


        /// <summary>
        /// Validates replay uri
        /// </summary>
        /// <returns>Original replay name</returns>
        /// <exception cref="InvalidDataException"></exception>
        private static async Task<string> GetReplayName()
        {
            try
            {
                //Assert.True(!string.IsNullOrEmpty(ReplayCdnUri));
                if (!Uri.TryCreate(PayloadSource.ReplayCdnUri, UriKind.RelativeOrAbsolute, out Uri? parsedUri) || parsedUri is null)
                    throw new InvalidDataException("ERROR: Replay uri is in an unsupported format.");

                using HttpClient httpClient = new(
                    new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 5
                    })
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                using HttpResponseMessage response = await httpClient.SendAsync(new(HttpMethod.Head, parsedUri));

                return (response.RequestMessage!.RequestUri is Uri requestUri && requestUri is not null ? requestUri : parsedUri).AbsolutePath.Split('/')[^1];
            }
            catch(Exception e)
            {
                await Console.Out.WriteLineAsync($"{CoreCompiler.NL}{e.Message}");
                CoreCompiler.ExitProgram("ERROR: Encountered while trying to resolve replay name.");
                return string.Empty;
            }
        }


        /// <summary>
        /// Validates project tree
        /// </summary>
        /// <returns>Root project directory</returns>
        private static DirectoryInfo? VerifyProjectTree()
        {
            DirectoryInfo? rootProjectDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent is DirectoryInfo tempBuild && tempBuild is not null
                                        ? (tempBuild.Parent is DirectoryInfo tempType && tempType is not null ? (tempType.Parent is DirectoryInfo tempBin ? tempBin : null) : null) : null;

            if (rootProjectDirectory is not null)
                return rootProjectDirectory;

            CoreCompiler.ExitProgram("ERROR: Could not navigate to root project directory.");
            return null;
        }


        /// <summary>
        /// Validates the path of the .ico file
        /// </summary>
        /// <param name="rootProjectDirectory"></param>
        private static void IconFileValidation(DirectoryInfo rootProjectDirectory)
        {
            string iconFilePath = Path.Combine(rootProjectDirectory.FullName, "icon", "icon.ico");
            if (!File.Exists(iconFilePath))
                CoreCompiler.ExitProgram($"ERROR: Icon file does not exist at {iconFilePath}.");
        }
    }
}
