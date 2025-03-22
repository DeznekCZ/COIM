using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ProgramableNetwork.PyModuleTest
{
    [TestClass]
    public class PythonImplementation
    {
        private Block Parse(params string[] v) => Lexer.Parse(Tokenizer.ParseLines(TraceFile(2), v));

        private FileInfo TraceFile(int back = 1)
        {
            return new FileInfo(new StackTrace().GetFrame(back).GetMethod().Name);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Block expression = Parse("return a + b");

            Assert.AreEqual(typeof(ReturnStatement), expression.statements[0].GetType());
            Assert.AreEqual(typeof(AddExpression), (expression.statements[0] as ReturnStatement).Expression.GetType());
            Assert.AreEqual(typeof(VariableExpression), ((expression.statements[0] as ReturnStatement).Expression as AddExpression).Left.GetType());
            Assert.AreEqual(typeof(VariableExpression), ((expression.statements[0] as ReturnStatement).Expression as AddExpression).Right.GetType());
            Assert.AreEqual("a", (((expression.statements[0] as ReturnStatement).Expression as AddExpression).Left as VariableExpression).Path);
            Assert.AreEqual("b", (((expression.statements[0] as ReturnStatement).Expression as AddExpression).Right as VariableExpression).Path);

        }

        [TestMethod]
        public void TestFString()
        {
            var context = new Dictionary<string, object>
            {
                { "a", "a" },
                { "b", "b" },
                { "c", "c" }
            };
            Block block = Parse(
                "return [",
                "    f\"\",",
                "    f\"{a} \",",
                "    f\" {b} {c}\",",
                "    f\" {b}{c} \"",
                "]"
            );
            try
            {
                block.statements[0].Execute(context);
                Assert.IsTrue(false, "Return was not called");
            }
            catch (ReturnException r)
            {
                Assert.IsTrue(r.Value is List<object>);
                Assert.AreEqual("", (r.Value as List<object>)[0] as string);
                Assert.AreEqual("a ", (r.Value as List<object>)[1] as string);
                Assert.AreEqual(" b c", (r.Value as List<object>)[2] as string);
                Assert.AreEqual(" bc ", (r.Value as List<object>)[3] as string);
            }
        }
    }
}
