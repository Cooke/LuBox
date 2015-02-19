using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBox.Test
{
    [TestClass]
    public class LuTableTests
    {
        private LuScriptEngine _engine;
        private LuTable _table;

        [TestInitialize]
        public void Initialize()
        {
            _engine = new LuScriptEngine();
            _table = new LuTable();
            _engine.DefaultEnvironment.SetField("table", _table);
        }

        [TestMethod]
        public void GetValue()
        {
            _table.SetField("value", 123);
            Assert.AreEqual(123, _engine.Evaluate("table.value"));
        }

        [TestMethod]
        public void SetValue()
        {
            _engine.Execute("table.value = 1");
            Assert.AreEqual(1, _table.GetField("value"));
        }

        [TestMethod]
        public void GetValueIndex()
        {
            _table.SetField("value", 123);
            Assert.AreEqual(123, _engine.Evaluate("table[\"value\"]"));
        }

        [TestMethod]
        public void SetValueIndex()
        {
            _engine.Execute("table[\"value\"] = 1");
            Assert.AreEqual(1, _table.GetField("value"));
        }
    }
}
