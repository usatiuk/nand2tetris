using Microsoft.VisualStudio.TestTools.UnitTesting;
using JackAnalyzer;

namespace MetaSymbolTableTest
{
    [TestClass]
    public class MetaSymbolTableTest
    {
        [TestMethod]
        public void CounterTest()
        {
            MetaSymbolTable st = new MetaSymbolTable();

            st.Define("x", "int", SymbolKind.Var);
            Assert.AreEqual(1, st.VarCount(SymbolKind.Var));
            Assert.AreEqual(0, st.IndexOf("x"));

            st.Define("y", "String", SymbolKind.Var);
            Assert.AreEqual(2, st.VarCount(SymbolKind.Var));
            Assert.AreEqual(1, st.IndexOf("y"));

            st.Define("z", "String", SymbolKind.Field);
            Assert.AreEqual(1, st.VarCount(SymbolKind.Field));
            Assert.AreEqual(0, st.IndexOf("z"));
        }

        public void ResetTest()
        {
            MetaSymbolTable st = new MetaSymbolTable();

            st.Define("x", "int", SymbolKind.Var);
            Assert.AreEqual(1, st.VarCount(SymbolKind.Var));
            Assert.AreEqual(0, st.IndexOf("x"));

            st.ResetFunctionTable();
            try
            {
                st.IndexOf("x");
                Assert.Fail();
            } catch { }
        }
    }
}
