using System;
using System.IO;
using System.Text;
using System.Linq;
using Tokenizer;

namespace Acidic
{
    class Program
    {
        
        const int EXIT_SUCCESS = 0;
        const int EXIT_FAILURE = 1;

        static void prepFile(string outFile)
        {
            if (File.Exists(outFile))
                File.WriteAllText(outFile, "");
            else
                File.Create(outFile);
        }

        static string[] readFile(string filename)
        {
            string[] lines;
            string[] returnArray;
            if (File.Exists(filename)){
                // read lines from file and return line with no null space
                lines = File.ReadAllLines(filename);
                returnArray = lines.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                return returnArray;
            }
            Environment.Exit(EXIT_FAILURE);
            return null;
        
        }

        static void Main(string[] args)
        {
            string filename = "test2.aci";//args[0];
            if (!File.Exists(filename)) { Console.WriteLine("File does not exist");Environment.Exit(EXIT_FAILURE); }
            string outfile = filename.Replace(".aci", ".acc");
            prepFile(outfile);
            string[] lines = readFile(filename);
            Token.TokenRunner(lines, outfile);
            Environment.Exit(EXIT_SUCCESS);
        }
    }
}
