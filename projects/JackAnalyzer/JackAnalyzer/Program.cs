using System;
using System.Collections.Generic;
using System.IO;

namespace JackAnalyzer
{
    class Program
    {

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Not enough arguments");
                return 1;
            }

            string inPath = args[0];

            List<string> inFiles = new List<string>();

            if (Directory.Exists(inPath))
            {
                string[] files = Directory.GetFiles(inPath);
                foreach(string file in files)
                {
                    if(Path.GetExtension(file) == Constants.inExt)
                    {
                        inFiles.Add(file);
                    }
                }
            }
            else if (File.Exists(inPath))
            {
                if (Path.GetExtension(inPath) == Constants.inExt)
                {
                    inFiles.Add(inPath);
                }
            }
            else
            {
                Console.WriteLine("No input files");
                return 2;
            }

            foreach(string file in inFiles)
            {
                Tokenizer tokenizer = new Tokenizer(file);
                tokenizer.WriteTokens(file);
                var tokens = tokenizer.Tokens;
                Writer writer = new Writer(file, tokens);
            }

            return 0;
        }
    }
}
