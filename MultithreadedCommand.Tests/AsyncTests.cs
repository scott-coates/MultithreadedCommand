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
    public class AsyncTests
    {
        public AsyncTests()
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
            new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).RemoveAll();
        }
        //
        #endregion

        #region Container
        [TestMethod]
        public void AsyncManagerAddsAsyncJobToContainer()
        {
            //arrange
            int requestId = 0;
            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            int originalCount = container.Count;

            ICommand a = new AsyncManager(func.Object, requestId.ToString(), container);

            Assert.IsTrue(new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count == originalCount + 1);
        }

        [TestMethod]
        public void RemovingRunningJobReducesCount()
        {

            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);
            int originalCount = container.Count;

            container.Remove("", a.DecoratedCommand.GetType());
            int newCount = container.Count;

            Assert.AreEqual(originalCount - 1, newCount);
        }

        [TestMethod]
        public void JobExistsWithAssemblyQualifiedName()
        {
            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            bool exists = container.Exists("", a.DecoratedCommand.GetType());

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void ContaierDecreasesWhenJobFinishes()
        {
            var wait = new ManualResetEvent(false);

            //arrange

            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            //act
            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });
            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true});

            int originalCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;
            new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => wait.Set();
            a.Start();

            //assert
            wait.WaitOne(3000);

            int finishedCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;
            Assert.IsTrue(finishedCount == originalCount - 1);
        }

        [TestMethod]
        public void RemovalTimerSetToCorrectRemovalMinutesWhenAdded()
        {
            var func = new Mock<ICommand>();
            AsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);
            TimeSpan removalTime = Properties.Settings.Default.ContainerRemovalTime;

            AsyncContainerItem item = container.GetContainerItem("", func.Object.GetType());

            Assert.AreEqual(item.Timer.Interval, removalTime.TotalMilliseconds);
        }

        [TestMethod]
        public void RemovalTimerStartsWhenJobIsFinished()
        {
            var func = new Mock<CommandBase> { CallBase = true };
            var wait = new ManualResetEvent(false);

            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = false });

            AsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());

            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            a.AfterSetActionInactive += () => wait.Set(); //set after the async manager is created.

            a.Start();

            wait.WaitOne(3000);

            AsyncContainerItem item = container.GetContainerItem("", func.Object.GetType());
            Assert.IsTrue(item.Timer.Enabled);
        }

        [TestMethod]
        public void RemovalTimerStartsWhenJobIsAdded()
        {
            var func = new Mock<CommandBase> { CallBase = true };

            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = false });

            AsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());

            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            AsyncContainerItem item = container.GetContainerItem("", func.Object.GetType());
            Assert.IsTrue(item.Timer.Enabled);
        }

        [TestMethod]
        public void JobRemovedFromContainerAfterRemovalTimeElapsed()
        {
            var waitForRemove = new ManualResetEvent(false);
            var func = new Mock<CommandBase>() { CallBase = true };
            var timeSpan = new Mock<IAsyncCommandItemTimeSpan>();
            timeSpan.Setup(t => t.Time).Returns(new TimeSpan(0, 0, 0, 0, 1));
            var container = new Mock<AsyncCommandContainer>(timeSpan.Object) { CallBase = true };
            
            int inactiveCalledCounter = 0;
            Action setInactive = () => 
            {
                if (++inactiveCalledCounter == 2)
                {
                    container.Object.GetContainerItem("", func.Object.GetType()).Timer.Start();
                }
            };

            container.Setup(c => c.SetInactive("",func.Object.GetType())).Callback(setInactive);
            IAsyncCommand a = new AsyncManager(func.Object, "", container.Object);

            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = false });

            new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => waitForRemove.Set();

            int originalCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;

            a.Start();
            waitForRemove.WaitOne(3000);

            int newCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;

            Assert.AreEqual(newCount, originalCount - 1);
        }

        [TestMethod]
        public void JobRemovedIfNotStartedForALongTime()
        {
            var waitForRemove = new ManualResetEvent(false);
            var func = new Mock<CommandBase>() { CallBase = true };
            var timeSpan = new Mock<IAsyncCommandItemTimeSpan>();
            timeSpan.Setup(t => t.Time).Returns(new TimeSpan(0, 0, 0, 0, 1));
            AsyncCommandContainer container = new AsyncCommandContainer(timeSpan.Object);
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            container.GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => waitForRemove.Set();

            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = false });

            waitForRemove.WaitOne(3000);

            Assert.IsFalse(container.Exists("",func.Object.GetType()));
        }

        [TestMethod]
        public void JobCounterResetWhenRetrieved()
        {
            var func = new Mock<IAsyncCommand>();

            var container = new Mock<AsyncCommandContainer>(new AsyncCommandItemTimeSpan()) { CallBase = true };

            container.Object.Add(func.Object, "", func.Object.GetType());

            var asyncCommand = container.Object.Get("", func.Object.GetType());

            container.Verify(c => c.ResetTimer(It.IsAny<string>(), It.IsAny<Type>()));
        }
        #endregion Container

        #region AsyncManager

        [TestMethod]
        public void AsyncFuncDoesNotRunWhenRunning()
        {
            //arrange
            var func = new Mock<ICommand>(MockBehavior.Strict);
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            ICommand a = new AsyncManager(func.Object, "", container);
            
            //act
            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.Running });
            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });

            a.Start();
            //exception will occur if inner func.Start is called.
        }

        [TestMethod]
        public void AsyncFuncDoesRunWhenNotRunning()
        {
            var wait = new ManualResetEvent(false);

            //arrange

            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            //act
            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });

            new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => wait.Set();

            a.Start();

            //assert
            wait.WaitOne(3000);
            func.Verify(f => f.Start());
        }
        //#endregion

        [TestMethod]
        public void AsyncManagerAddsStartEventToInnerClass()
        {
            //arrange
            var mock = new Rhino.Mocks.MockRepository();
            var func = mock.DynamicMock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
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
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
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
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            var asyncFunc = new Mock<ICommand>().Object.AsAsync("", container);
            Assert.IsTrue(asyncFunc is AsyncManager);
        }

        [TestMethod]
        public void ExtensionMethodAddsFuncToAsyncFuncContainer()
        {
            int originalCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            var asyncFunc = new Mock<ICommand>().Object.AsAsync("", container);

            Assert.IsTrue(originalCount + 1 == new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count);
        }

        [TestMethod]
        public void FuncNotAsyncAndNotAddedToAsyncContainer()
        {
            int originalCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;
            var asyncFunc = new Mock<ICommand>();

            Assert.IsTrue(originalCount == new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count);
        }

        [TestMethod]
        public void CallingCancelTellsInnerClassToCancel()
        {
            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            a.Cancel();
            func.Verify(f => f.Cancel(), Times.Exactly(1)); //even though we call a.cancel, func.Cancel is eventually called.
        }

        #endregion

        #region CommandBase
        [TestMethod]
        public void CallingCancelReturnsCancelledStatus()
        {
            var func = new Mock<CommandBase>() { CallBase = true };

            func.Setup(f => f.CoreStart()).Callback(() => func.Object.Cancel()); //the derived proxy class calls cancel for it's core action.

            func.Object.Start();

            Assert.AreEqual(func.Object.Progress.Status, StatusEnum.Cancelled);
        }

        [TestMethod]
        public void OnSuccessNotCalledWhenCancelled()
        {
            var wait = new ManualResetEvent(false);
            bool shouldBeTrue = true;

            var func = new Mock<CommandBase>() { CallBase = true };
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            a.OnSuccess += () => shouldBeTrue = false;
            a.OnEnd += () => wait.Set();

            func.Setup(f => f.CoreStart()).Callback(() => func.Object.Cancel());

            a.Start();

            wait.WaitOne(3000);
            Assert.IsTrue(shouldBeTrue);
        }

        [TestMethod]
        public void OnSuccessCalledWhenNotCancelled()
        {
            var wait = new ManualResetEvent(false);
            bool shouldBeTrue = false;

            var func = new Mock<CommandBase>() { CallBase = true };

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);

            a.OnSuccess += () => shouldBeTrue = true;

            new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => wait.Set();

            a.Start();

            wait.WaitOne(3000);
            Assert.IsTrue(shouldBeTrue);
        }

        [TestMethod]
        public void RemoveFromContainerDuringRunReducesCount()
        {
            var wait = new ManualResetEvent(false);
            var func = new Mock<CommandBase>() { CallBase = true };
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);
            
            new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => wait.Set();

            int originalCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;

            func.Setup(f => f.CoreStart()).Callback(() => new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Remove("", func.Object.GetType()));

            a.Start();

            wait.WaitOne(3000);

            int newCount = new AsyncCommandContainer(new AsyncCommandItemTimeSpan()).Count;

            Assert.AreEqual(originalCount - 1, newCount);
        }
        #endregion

        #region CommandRemover

        [TestMethod]
        public void RemoverRemovesJob()
        {
            var func = new Mock<ICommand>();
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);
            IAsyncCommandRemover remover = new AsyncCommandRemover(container);
            int originalCount = container.Count;

            remover.RemoveCommand("", func.Object.GetType());

            int newCount = container.Count;

            Assert.AreEqual(originalCount - 1, newCount);
        }

        [TestMethod]
        public void RemoverCancelsJob()
        {
            var wait = new ManualResetEvent(false);
            var func = new Mock<CommandBase> { CallBase = true };
            var container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            IAsyncCommand a = new AsyncManager(func.Object, "", container);
            IAsyncCommandRemover remover = new AsyncCommandRemover(container);

            func.Setup(f => f.CoreStart()).Callback(() => remover.RemoveCommand("", func.Object.GetType()));

            container.GetContainerItem("", func.Object.GetType()).OnItemRemoved += () => wait.Set();

            a.Start();

            wait.WaitOne(3000);

            Assert.AreEqual(StatusEnum.Cancelled, func.Object.Progress.Status);
        }

        #endregion
    }
}
