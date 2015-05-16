using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuBox.Compiler;

namespace LuBox.Test
{
    [TestClass]
    public class CommentTests
    {
        private LuScriptEngine _luScriptEngine;
        private LuTable _environment;

        [TestInitialize]
        public void Initialize()
        {
            _luScriptEngine = new LuScriptEngine();
            _environment = _luScriptEngine.CreateStandardEnvironment();
        }

        [TestMethod]
        public void OnlyCommands()
        {
            _luScriptEngine.Execute("-- Only comment");
        }
    }
}