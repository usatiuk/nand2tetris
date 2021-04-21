using System;
using System.Collections.Generic;
using System.Text;

namespace JackAnalyzer
{
    public class MetaSymbolTable
    {
        private SymbolTable ClassSymbolTable = new SymbolTable();
        private SymbolTable FunctionSymbolTable = new SymbolTable();

        public MetaSymbolTable()
        {

        }

        public void Define(string name, string type, SymbolKind kind)
        {
            if (kind == SymbolKind.Field || kind == SymbolKind.Static)
            {
                ClassSymbolTable.Define(name, type, kind);
            }
            else
            {
                FunctionSymbolTable.Define(name, type, kind);
            }
        }

        internal void ResetFieldTable()
        {
            ClassSymbolTable.ResetFields();
        }

        public int VarCount(SymbolKind kind)
        {
            if (kind == SymbolKind.Field || kind == SymbolKind.Static)
            {
                return ClassSymbolTable.VarCount(kind);
            }
            else
            {
                return FunctionSymbolTable.VarCount(kind);
            }
        }

        public SymbolKind KindOf(string name)
        {
            try
            {
                return FunctionSymbolTable.KindOf(name);
            }
            catch
            {
                return ClassSymbolTable.KindOf(name);
            }
        }

        public string TypeOf(string name)
        {
            try
            {
                return FunctionSymbolTable.TypeOf(name);
            }
            catch
            {
                return ClassSymbolTable.TypeOf(name);
            }
        }

        public int IndexOf(string name)
        {
            try
            {
                return FunctionSymbolTable.IndexOf(name);
            }
            catch
            {
                return ClassSymbolTable.IndexOf(name);
            }
        }

        public void ResetFunctionTable()
        {
            FunctionSymbolTable = new SymbolTable();
        }
    }
}
