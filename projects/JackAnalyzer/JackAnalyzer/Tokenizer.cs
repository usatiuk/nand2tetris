using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Security;

namespace JackAnalyzer
{
    class Tokenizer
    {
        public List<KeyValuePair<TokenType, string>> Tokens { get; }

        public Tokenizer(string file)
        {
            Tokens = new List<KeyValuePair<TokenType, string>>();

            string[] text = File.ReadAllLines(file);

            string word = "";

            bool readingComment = false;

            foreach (string line in text)
            {
                // Ignore comments and empty lines
                if (line.Length > 0)
                {
                    bool readingString = false;

                    // This is used for comment detection
                    char prevSymbol = '\0';

                    foreach (char c in line)
                    {
                        if (readingComment)
                        {
                            if (c == '/' && prevSymbol == '*')
                            {
                                readingComment = false;
                                continue;
                            }
                            else
                            {
                                prevSymbol = c;
                                continue;
                            }
                        }
                        if (c == '"')
                        {
                            if (readingString)
                            {
                                readingString = false;

                                PutStringConst(word);
                                word = "";
                                continue;
                            }
                            else
                            {
                                readingString = true;
                                continue;
                            }
                        }

                        if (readingString)
                        {
                            word += c;
                            continue;
                        }

                        if (Constants.symbols.Contains(c))
                        {
                            // Detect comments
                            if (prevSymbol == '/' && c == '/')
                            {
                                // If we're looking at a comment, that means
                                // we need to remove the previous symbol and
                                // discard the rest of the string
                                if (Tokens.Count > 0)
                                {
                                    Tokens.RemoveAt(Tokens.Count - 1);
                                }
                                break;
                            }
                            if (prevSymbol == '/' && c == '*')
                            {
                                // If we're looking at a multi-line, that means
                                // we need to remove the previous symbol and
                                // discard the rest of the comment
                                if (Tokens.Count > 0)
                                {
                                    Tokens.RemoveAt(Tokens.Count - 1);
                                }
                                readingComment = true;
                                continue;
                            }

                            prevSymbol = c;

                            PutString(word);
                            PutString(char.ToString(c));
                            word = "";
                            continue;
                        }

                        word += c;

                        if (c == ' ')
                        {
                            PutString(word);
                            word = "";
                        }
                    }

                }

            }
        }

        public void WriteTokens(string path)
        {
            string filename = Path.GetDirectoryName(path) + '/' + Path.GetFileNameWithoutExtension(path) + "T.xml";
            using (StreamWriter file = new StreamWriter(filename))
            {
                file.WriteLine("<tokens>");
                foreach (var token in Tokens)
                {
                    var name = Enum.GetName(typeof(TokenType), token.Key);
                    var value = token.Value;
                    file.WriteLine('<' + name + "> " + SecurityElement.Escape(value) + " </" + name + '>');
                }
                file.WriteLine("</tokens>");
            }
        }

        private void PutStringConst(string token)
        {
            var str = new KeyValuePair<TokenType, string>(TokenType.stringConstant, token);
            Tokens.Add(str);
            return;
        }

        private void PutString(string token)
        {
            var trimmed = token.Trim();

            if (trimmed.Length == 1 && Constants.symbols.Contains(trimmed[0]))
            {
                var symbol = new KeyValuePair<TokenType, string>(TokenType.symbol, trimmed);
                Tokens.Add(symbol);
                return;
            }

            if (Constants.keywords.Contains(trimmed))
            {
                var keyword = new KeyValuePair<TokenType, string>(TokenType.keyword, trimmed);
                Tokens.Add(keyword);
                return;
            }

            if (int.TryParse(trimmed, out _))
            {
                var integerConstant = new KeyValuePair<TokenType, string>(TokenType.integerConstant, trimmed);
                Tokens.Add(integerConstant);
                return;
            }

            if (trimmed.Length > 0)
            {
                var identifier = new KeyValuePair<TokenType, string>(TokenType.identifier, trimmed);
                Tokens.Add(identifier);
                return;
            }

        }

    }
}
