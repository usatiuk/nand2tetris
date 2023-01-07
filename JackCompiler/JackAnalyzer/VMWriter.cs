using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JackAnalyzer
{
    enum Segment
    {
        Constant, Argument, Local, Static, This, That, Pointer, Temp
    }

    enum ArithmeticOp
    {
        Add, Sub, Neg, Eq, Gt, Lt, And, Or, Not
    }
    class VMWriter
    {
        StreamWriter file;
        List<string> cache = new List<string>();
        int funcDef = 0;

        public VMWriter(string path)
        {
            string filename = Path.ChangeExtension(path, ".vm");
            file = new StreamWriter(filename);
            file.AutoFlush = true;
        }

        public void Dispose()
        {       
            foreach (string l in cache)
            {
                file.WriteLine(l);
            }
            file.Close();
            file.Dispose();
        }

        public void WritePush(Segment segment, int index)
        {
            cache.Add($"push {Enum.GetName(typeof(Segment),segment).ToLower()} {index}");
        }

        public void WritePop(Segment segment, int index)
        {
            cache.Add($"pop {Enum.GetName(typeof(Segment), segment).ToLower()} {index}");
        }

        public void WriteArithmetic(ArithmeticOp op)
        {
            cache.Add(Enum.GetName(typeof(ArithmeticOp), op).ToLower());
        }

        public void WriteLabel(string label)
        {
            cache.Add($"label {label}");
        }

        public void WriteGoto(string label)
        {
            cache.Add($"goto {label}");
        }

        public void WriteIf(string label)
        {
            cache.Add($"if-goto {label}");
        }

        public void WriteCall(string name, int nargs)
        {
            cache.Add($"call {name} {nargs}");
        }

        public void WriteFunction(string name, int nlocal)
        {
            cache.Add($"function {name} {nlocal}");
            funcDef = cache.Count - 1;
        }

        public void RewriteFunctionLocals(string name, int nlocal)
        {
            cache[funcDef]=($"function {name} {nlocal}");
        }

        public void WriteReturn()
        {
            cache.Add("return");
        }

    }
}
