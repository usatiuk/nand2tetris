using Microsoft.VisualStudio.TestTools.UnitTesting;
using JackAnalyzer;

namespace SymbolTableTest
{
    [TestClass]
    public class SymbolTableTest
    {
        [TestMethod]
        public void CounterTest()
        {
            SymbolTable st = new SymbolTable();

            st.Define("x", "int", SymbolKind.Var);
            Assert.AreEqual(1, st.VarCount(SymbolKind.Var));
            Assert.AreEqual(0, st.IndexOf("x"));

            st.Define("y", "String", SymbolKind.Var);
            Assert.AreEqual(2, st.VarCount(SymbolKind.Var));
            Assert.AreEqual(1, st.IndexOf("y"));

            st.Define("z", "String", SymbolKind.Field);
            Assert.AreEqual(1, st.VarCount(SymbolKind.Field));
            Assert.AreEqual(0, st.IndexOf("z"));

            Assert.AreEqual(0, st.VarCount(SymbolKind.Static));
        }
    }
}
