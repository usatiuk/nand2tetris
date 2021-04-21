using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JackAnalyzer
{
    public enum SymbolKind
    {
        Static,
        Field,
        Arg,
        Var,
        None
    }
    public class SymbolTable
    {
        Dictionary<string, ValueTuple<string, SymbolKind, int>> table = new Dictionary<string, (string, SymbolKind, int)>();
        Dictionary<SymbolKind, int> kindCounter = new Dictionary<SymbolKind, int>();

        public SymbolTable()
        {
        }

        public void Define(string name, string type, SymbolKind kind)
        {
            int count;
            kindCounter.TryGetValue(kind, out count);
            kindCounter[kind] = count + 1;

            table.Add(name, (type, kind, count));
        }

        public int VarCount(SymbolKind kind)
        {
            try
            {
            return kindCounter[kind];
            } catch
            {
                return 0;
            }
        }

        public SymbolKind KindOf(string name)
        {
            return table[name].Item2;
        }

        public string TypeOf(string name)
        {
            return table[name].Item1;
        }

        public int IndexOf(string name)
        {
            return table[name].Item3;
        }

        public void ResetFields()
        {
            kindCounter[SymbolKind.Field] = 0;
            var itemsToRemove = table.Where(f => f.Value.Item2 == SymbolKind.Field).ToArray();
            foreach (var item in itemsToRemove)
            {
                table.Remove(item.Key);
            }
        }
    }
}
