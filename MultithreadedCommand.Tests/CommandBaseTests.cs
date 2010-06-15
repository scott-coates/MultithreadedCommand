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
    public class CommandBaseTests
    {
        public CommandBaseTests()
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

        #region CommandBase
        [TestMethod]
        public void CallingCancelReturnsCancelledStatus()
        {
            var func = new Mock<CommandBase> { CallBase = true };

            func.Setup(f => f.CoreStart()).Callback(() => func.Object.Cancel()); //the derived proxy class calls cancel for it's core action.

            func.Object.Start();

            Assert.AreEqual(func.Object.Progress.Status, StatusEnum.Cancelled);
        }

        [TestMethod]
        public void OnSuccessNotCalledWhenCancelled()
        {
            bool shouldBeTrue = true;

            var func = new Mock<CommandBase> { CallBase = true };
            var container = new Mock<IAsyncCommandContainer>();

            func.Object.OnSuccess += () => shouldBeTrue = false;

            func.Setup(f => f.CoreStart()).Callback(() => func.Object.Cancel());

            func.Object.Start();

            Assert.IsTrue(shouldBeTrue);
        }

        [TestMethod]
        public void OnSuccessCalledWhenNotCancelled()
        {
            bool shouldBeTrue = false;

            var func = new Mock<CommandBase> { CallBase = true };

            func.Setup(f => f.Properties.ShouldBeRemovedOnComplete).Returns(true);

            var container = new Mock<IAsyncCommandContainer>();

            func.Object.OnSuccess += () => shouldBeTrue = true;

            func.Object.Start();

            Assert.IsTrue(shouldBeTrue);
        }
        #endregion
    }
}
