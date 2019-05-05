using System;
using System.Linq;
using System.Diagnostics;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static bool ContainsNegation(this string word)
        {
            var negationWords = new string[] { "not", "no", "neither", "never", "nobody", "none", "no one", "nothing", "without", "n't" };
            return negationWords.Contains(word);
        }

        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                }
            };
            
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}