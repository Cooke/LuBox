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

        [TestMethod]
        public void CreateEmptyTable()
        {
            _engine.Execute("table =  { }");
            Assert.IsNotNull(_engine.DefaultEnvironment.Dynamic.table);
        }

        [TestMethod]
        public void CreateTable()
        {
            _engine.Execute("table =  { hej = 1 }");
            Assert.AreEqual(1, _engine.DefaultEnvironment.Dynamic.table.hej);
        }

        [TestMethod]
        public void CreateTableExpressionIndex()
        {
            _engine.Execute("local var = 'hej'; table =  { [var] = 1 }");
            Assert.AreEqual(1, _engine.DefaultEnvironment.Dynamic.table.hej);
        }

        [TestMethod]
        public void CreateTableNoIndex()
        {
            _engine.Execute("table =  { 1, 2 }");
            Assert.AreEqual(2, _engine.DefaultEnvironment.Dynamic.table[2]);
        }

        [TestMethod]
        public void AutoConvertToDTO()
        {
            var test = new Test();
            _engine.DefaultEnvironment.Dynamic.test = test;
            _engine.Execute("test.Foo({ StringValue = 'geh', IntValue = 123 })");
            Assert.AreEqual(123, test.dto.IntValue);
            Assert.AreEqual("geh", test.dto.StringValue);
        }

        public class Test
        {
            public void Foo(DTO dto)
            {
                this.dto = dto;
            }

            public DTO dto { get; set; }
        }

        public class DTO
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }
        }
    }
}
