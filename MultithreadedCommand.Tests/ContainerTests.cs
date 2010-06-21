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
using CaresCommon.Logging;

namespace MultithreadedCommand.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ContainerTests
    {
        public ContainerTests()
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
        [TestCleanup()]
        public void MyTestCleanup()
        {
            new AsyncCommandContainer(new Mock<IAsyncCommandItemTimeSpan>().Object, new Mock<ILogger>().Object).RemoveAll();
        }
        //
        #endregion

        #region Container
        [TestMethod]
        public void RemovingRunningJobReducesCount()
        {
            ILogger logger = new Mock<ILogger>().Object;
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan(), logger);
            var async = new Mock<IAsyncCommand>();
            async.Setup(a => a.Progress.Status).ReturnsInOrder(StatusEnum.NotStarted, StatusEnum.Running);
            container.Add(async.Object, "", async.Object.GetType());
            int originalCount = container.Count;

            container.Remove("", async.Object.GetType());
            int newCount = container.Count;

            Assert.AreEqual(originalCount - 1, newCount);
        }

        [TestMethod]
        public void JobExistsWithAssemblyQualifiedName()
        {
            ILogger logger = new Mock<ILogger>().Object;
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan(), logger);

            var asyncFunc = new Mock<IAsyncCommand> { DefaultValue = DefaultValue.Mock };
            container.Add(asyncFunc.Object, "", asyncFunc.Object.DecoratedCommand.GetType());
            bool exists = container.Exists("", asyncFunc.Object.DecoratedCommand.GetType());

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void RemovalTimerSetToCorrectRemovalMinutesWhenAdded()
        {
            ILogger logger = new Mock<ILogger>().Object;
            AsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan(), logger);
            var asyncFunc = new Mock<IAsyncCommand> { DefaultValue = DefaultValue.Mock };

            TimeSpan removalTime = Properties.Settings.Default.ContainerRemovalTime;

            container.Add(asyncFunc.Object, "", asyncFunc.Object.DecoratedCommand.GetType());
            AsyncContainerItem item = container.GetContainerItem("", asyncFunc.Object.DecoratedCommand.GetType());

            Assert.AreEqual(item.Timer.Interval, removalTime.TotalMilliseconds);
        }

        [TestMethod]
        public void RemovalTimerStartsWhenJobIsAdded()
        {
            var func = new Mock<IAsyncCommand>();
            var logger = new Mock<ILogger>();

            func.Setup(f => f.Progress.Status).Returns(StatusEnum.NotStarted);

            AsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan(), logger.Object);

            IAsyncCommand a = new AsyncManager(func.Object, "", container,logger.Object);

            AsyncContainerItem item = container.GetContainerItem("", func.Object.GetType());
            Assert.IsTrue(item.Timer.Enabled);
        }

        [TestMethod]
        public void JobRemovedFromContainerAfterRemovalTimeElapsed()
        {
            var wait = new ManualResetEvent(false);
            var asyncFunc = new Mock<IAsyncCommand>();
            var timeSpan = new Mock<IAsyncCommandItemTimeSpan>();
            ILogger logger = new Mock<ILogger>().Object;

            asyncFunc.Setup(a => a.Progress.Status).Returns(StatusEnum.Finished);
            timeSpan.Setup(t => t.Time).Returns(new TimeSpan(0, 0, 0, 0, 1));

            var container = new AsyncCommandContainer(timeSpan.Object, logger);

            container.Add(asyncFunc.Object, "", asyncFunc.Object.GetType());

            container.SetInactive("", asyncFunc.Object.GetType());
            container.GetContainerItem("", asyncFunc.Object.GetType()).OnItemRemoved += () => wait.Set();

            wait.WaitOne(3000);

            Assert.IsFalse(container.Exists("", asyncFunc.Object.GetType()));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SettingInactiveJobWhenRunningThrowsException()
        {
            var func = new Mock<IAsyncCommand>();
            ILogger logger = new Mock<ILogger>().Object;

            func.Setup(f => f.Progress).Returns(new FuncStatus { Status = StatusEnum.Running });

            var container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan(), logger);

            container.Add(func.Object, "", func.Object.GetType());

            container.SetInactive("", func.Object.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SettingActiveJobWhenNotRunningThrowsException()
        {
            var func = new Mock<IAsyncCommand>();
            ILogger logger = new Mock<ILogger>().Object;

            func.Setup(f => f.Progress.Status).Returns(StatusEnum.Finished);

            var container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan(), logger);

            container.Add(func.Object, "", func.Object.GetType());

            container.SetActive("", func.Object.GetType());
        }
        #endregion Container
    }
}
