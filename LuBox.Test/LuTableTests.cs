﻿using System;
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
        public void SetValueIndexOnInnerTable()
        {
            _engine.Execute("table.innerTable = {}");
            _engine.Execute("table.innerTable[\"value\"] = 1");
            Assert.AreEqual(1, ((LuTable)_table.GetField("innerTable")).GetField("value"));
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
            _engine.Execute("test:Foo({ StringValue = 'geh', IntValue = 123 })");
            Assert.AreEqual(123, test.dto.IntValue);
            Assert.AreEqual("geh", test.dto.StringValue);
        }

        [TestMethod]
        public void AutoConvertToDTOWithFunction()
        {
            var test = new Test();
            _engine.DefaultEnvironment.Dynamic.test = test;
            _engine.Execute("test:Foo({ Func = function() return 12 end })");
            Assert.AreEqual(12, test.dto.Func());
        }

        [TestMethod]
        public void IterateIndexValue()
        {
            _engine.DefaultEnvironment.Dynamic.sum = 0;
            _table.SetField("one", 1);
            _table.SetField("two", 2);
            
            _engine.Execute("for i,v in ipairs(table) do sum = sum + i end");
            Assert.AreEqual(3, _engine.DefaultEnvironment.Dynamic.sum);
        }

        [TestMethod]
        public void IterateIndexValueSum()
        {
            _engine.DefaultEnvironment.Dynamic.sum = 0;
            _table.SetField("one", 1);
            _table.SetField("two", 2);

            _engine.Execute("for i,v in ipairs(table) do sum = sum + v end");
            Assert.AreEqual(3, _engine.DefaultEnvironment.Dynamic.sum);
        }

        [TestMethod]
        public void IterateKeyValue()
        {
            _engine.DefaultEnvironment.Dynamic.sum = 0;
            _table.SetField("one", 1);
            _table.SetField("two", 2);

            _engine.Execute("for k,v in pairs(table) do sum = sum + v end");
            Assert.AreEqual(3, _engine.DefaultEnvironment.Dynamic.sum);
        }

        [TestMethod]
        public void SetWithIntKey()
        {
            _engine.Execute("table[1] = 123");

            Assert.AreEqual(123, _engine.DefaultEnvironment.Dynamic.table[1]);
        }

        [TestMethod]
        public void GetWithIntKey()
        {
            _table.SetField(2, 123);

            Assert.AreEqual(123, _engine.Evaluate("table[2]"));
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

            public Func<int> Func { get; set; }
        }
    }
}
