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
    public class AsyncManagerTests
    {
        public AsyncManagerTests()
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

        #region AsyncManager

        [TestMethod]
        public void AsyncManagerAddsAsyncJobToContainer()
        {
            //arrange
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();

            IAsyncCommand a = new AsyncManager(func.Object, "", container.Object);

            container.Verify(c => c.Add(a, "", func.Object.GetType()));
        }

        [TestMethod]
        public void AsyncFuncDoesNotRunWhenRunning()
        {
            //arrange
            var func = new Mock<ICommand>();

            func.Setup(f => f.Progress.Status).Returns(StatusEnum.Running);

            func.Setup(f => f.Properties.ShouldBeRemovedOnComplete).Returns(true);

            var container = new Mock<IAsyncCommandContainer>();
            ICommand a = new AsyncManager(func.Object, "", container.Object);

            //act
            a.Start();

            //assert
            func.Verify(f => f.Start(), Times.Never());
        }

        [TestMethod]
        public void AsyncFuncDoesRunWhenNotRunning()
        {
            //arrange
            var func = new Mock<ICommand>();

            func.Setup(f => f.Progress.Status).Returns(StatusEnum.NotStarted);

            func.Setup(f => f.Properties.ShouldBeRemovedOnComplete).Returns(true);

            var container = new Mock<IAsyncCommandContainer>();

            AsyncManager a = new AsyncManager(func.Object, "", container.Object);
            
            //act
            a.Start(false);

            //assert
            func.Verify(f => f.Start());
        }

        [TestMethod]
        public void AsyncManagerAddsStartEventToInnerClass()
        {
            //arrange
            var mock = new Rhino.Mocks.MockRepository();
            var func = mock.DynamicMock<ICommand>();
            var container = mock.DynamicMock<IAsyncCommandContainer>();
            IAsyncCommand a = new AsyncManager(func, "", container);
            Action set = () => { };

            //act
            func.Replay();
            a.OnStart += set;

            //assert
            func.AssertWasCalled(x => x.OnStart += set);
        }

        [TestMethod]
        public void AsyncManagerRemovesEventToInnerClass()
        {
            //arrange
            var mock = new MockRepository();
            var func = mock.DynamicMock<ICommand>();
            var container = mock.DynamicMock<IAsyncCommandContainer>();
            IAsyncCommand a = new AsyncManager(func, "", container);
            Action set = () => { };

            //act
            func.Replay();
            a.OnStart += set;
            a.OnStart -= set;

            //assert
            func.AssertWasCalled(x => x.OnStart += set);
            func.AssertWasCalled(x => x.OnStart -= set);
        }

        [TestMethod]
        public void ExtensionMethodReturnsAsyncManager()
        {
            var container = new Mock<IAsyncCommandContainer>();
            var asyncFunc = new Mock<ICommand>().Object.AsAsync("", container.Object);
            Assert.IsTrue(asyncFunc is AsyncManager);
        }

        [TestMethod]
        public void ExtensionMethodAddsFuncToAsyncFuncContainer()
        {
            var container = new Mock<IAsyncCommandContainer>();
            var asyncFunc = new Mock<ICommand>().Object.AsAsync("", container.Object);

            container.Verify(c => c.Add(It.IsAny<IAsyncCommand>(), "", It.IsAny<Type>()), Times.Once());
        }

        [TestMethod]
        public void FuncNotAsyncAndNotAddedToAsyncContainer()
        {
            var container = new Mock<IAsyncCommandContainer>();
            var asyncFunc = new Mock<ICommand>();

            container.Verify(c => c.Add(It.IsAny<IAsyncCommand>(), "", It.IsAny<Type>()), Times.Never());
        }

        [TestMethod]
        public void CallingCancelTellsInnerClassToCancel()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();
            IAsyncCommand a = new AsyncManager(func.Object, "", container.Object);

            a.Cancel();
            func.Verify(f => f.Cancel(), Times.Exactly(1)); //even though we call a.cancel, func.Cancel is eventually called.
        }

        [TestMethod]
        public void JobRemovedWhenFinishesAndShouldBeRemovedOnCompleteIsTrue()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();
            AsyncManager a = new AsyncManager(func.Object, "", container.Object);

            //act
            func.Setup(f => f.Progress.Status).Returns(StatusEnum.NotStarted);
            func.Setup(f => f.Properties.ShouldBeRemovedOnComplete).Returns(true);

            a.Start(runAsync: false);

            //assert
            container.Verify(c => c.Remove("", func.Object.GetType()));
        }

        [TestMethod]
        public void JobNotRemovedWhenFinishesAndShouldBeRemovedOnCompleteIsFalse()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();
            IAsyncCommand a = new AsyncManager(func.Object, "", container.Object);

            //act
            func.Setup(f => f.Progress.Status).Returns(StatusEnum.NotStarted);
            func.Setup(f => f.Properties.ShouldBeRemovedOnComplete).Returns(false);

            a.Start();

            //assert
            container.Verify(c => c.Remove("", func.Object.GetType()), Times.Never());
        }

        [TestMethod]
        public void RemovalTimerStartsWhenJobIsFinished()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();

            func.Setup(f => f.Progress.Status).ReturnsInOrder(StatusEnum.NotStarted, StatusEnum.Running);
            func.Setup(f => f.Properties.ShouldBeRemovedOnComplete).Returns(false);
            container.Setup(c => c.Exists(It.IsAny<string>(), It.IsAny<Type>())).Returns(true);
            var asyncFunc = new AsyncManager(func.Object, "", container.Object);

            asyncFunc.Start();

            container.Verify(c => c.SetInactive("", func.Object.GetType()));
        }

        [TestMethod]
        public void RemovalTimerStopsWhenJobStartsRunning()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();

            func.Setup(f => f.Progress.Status).Returns(StatusEnum.Running);
            func.Setup(f => f.Start()).Callback(() => container.Verify(c => c.SetActive("", func.Object.GetType())));
            container.Setup(c => c.Exists(It.IsAny<string>(), It.IsAny<Type>())).Returns(true);
            var asyncFunc = new AsyncManager(func.Object, "", container.Object);

            asyncFunc.Start();
        }

        [TestMethod]
        public void JobSetToInactiveWhenAdded()
        {
            var func = new Mock<ICommand>();
            var container = new Mock<IAsyncCommandContainer>();

            AsyncManager manager = new AsyncManager(func.Object, "", container.Object);

            container.Verify(c => c.SetInactive("", func.Object.GetType()));

            Assert.IsFalse(container.Object.Exists("", func.Object.GetType()));
        }
        #endregion
    }
}
