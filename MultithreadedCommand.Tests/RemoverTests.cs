using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Moq;
using System.Threading;
using MultithreadedCommand.Core.Async;
using MultithreadedCommand.Core.Commands;

namespace MultithreadedCommand.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class RemoverTests
    {
        public RemoverTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext) {}
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup() { }
        //
        #endregion
        
        #region CommandRemover

        [TestMethod]
        public void RemoverRemovesJob()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();
            var asyncFunc = new Mock<IAsyncCommand>();
            IAsyncCommandRemover remover = new AsyncCommandRemover(container.Object);

            container.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<Type>())).Returns(asyncFunc.Object);

            remover.RemoveCommand("", func.Object.GetType());

            container.Verify(c => c.Remove("", func.Object.GetType()));
        }

        [TestMethod]
        public void RemoverCancelsJob()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();
            var asyncFunc = new Mock<IAsyncCommand>();
            IAsyncCommandRemover remover = new AsyncCommandRemover(container.Object);

            container.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<Type>())).Returns(asyncFunc.Object);

            remover.RemoveCommand("", func.Object.GetType());

            asyncFunc.Verify(af => af.Cancel());
        }

        #endregion
    }
}
