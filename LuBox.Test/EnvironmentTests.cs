using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class EnvironmentTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuEnvironment _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = new LuEnvironment();
        }

        [TestMethod]
        public void SetPublicVariable()
        {
            _luScriptEngine.Execute("result = 3 * 4", _environment);
            Assert.AreEqual(12, _environment.Variables.result);
        }

        [TestMethod]
        public void SetPublicVariableAndUseIt()
        {
            _luScriptEngine.Execute(@"
result = 3 * 4
result2 = result * 2
", _environment);
            Assert.AreEqual(24, _environment.Variables.result2);
        }

        [TestMethod]
        public void SetMemberOfGlobal()
        {
            var box = new VariableBox();
            _environment.Variables.box = box;
            _luScriptEngine.Execute(@"box.Prop1 = 12", _environment);
            Assert.AreEqual(12, box.Prop1);
        }

        [TestMethod]
        public void SetMemberOfMemberOfGlobal()
        {
            var box = new VariableBox();
            _environment.Variables.box = box;
            _luScriptEngine.Execute(@"box.NestedBox.Prop1 = 10", _environment);
            Assert.AreEqual(10, box.NestedBox.Prop1);
        }


        [TestMethod]
        public void SetFieldMemberOfGlobal()
        {
            var box = new VariableBox();
            _environment.Variables.box = box;
            _luScriptEngine.Execute(@"box.Field1 = ""12""", _environment);
            Assert.AreEqual("12", box.Field1);
        }

        [TestMethod]
        public void SetFieldMemberOfMemberOfGlobal()
        {
            var box = new VariableBox();
            _environment.Variables.box = box;
            _luScriptEngine.Execute(@"box.NestedBox.Field1 = ""10""", _environment);
            Assert.AreEqual("10", box.NestedBox.Field1);
        }

        private class VariableBox
        {
            private readonly Lazy<VariableBox> nestedBox = new Lazy<VariableBox>();

            public int Prop1 { get; set; }

            public string Field1;

            public VariableBox NestedBox
            {
                get { return nestedBox.Value; }
            }
        }
    }
}