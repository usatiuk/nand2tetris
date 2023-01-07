using System;
using System.Collections.Generic;
using System.Text;

namespace JackAnalyzer
{
    class Constants
    {
        public const string inExt = ".jack";

        public static readonly char[] symbols = { '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~' };
        public static readonly string[] keywords = { "class", "constructor", "function", "method", "field", "static", "var", "int", "char", "boolean", "void", "true", "false", "null", "this", "let", "do", "if", "else", "while", "return" };
        public static readonly char[] op = { '+', '-', '*', '/', '&', '|', '<', '>', '=' };
        public static readonly char[] unaryOp = { '-', '~' };

        public const int indentLevel = 2;
    }
}
