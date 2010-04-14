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
        public void RemovingRunningJobReducesCount()
        {
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
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
            IAsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());

            var asyncFunc = new Mock<IAsyncCommand> { DefaultValue = DefaultValue.Mock };

            container.Add(asyncFunc.Object, "", asyncFunc.Object.DecoratedCommand.GetType());
            bool exists = container.Exists("", asyncFunc.Object.DecoratedCommand.GetType());

            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void RemovalTimerSetToCorrectRemovalMinutesWhenAdded()
        {
            AsyncCommandContainer container = new AsyncCommandContainer(new AsyncCommandItemTimeSpan());
            var asyncFunc = new Mock<IAsyncCommand> { DefaultValue = DefaultValue.Mock };

            TimeSpan removalTime = Properties.Settings.Default.ContainerRemovalTime;

            container.Add(asyncFunc.Object, "", asyncFunc.Object.DecoratedCommand.GetType());
            AsyncContainerItem item = container.GetContainerItem("", asyncFunc.Object.DecoratedCommand.GetType());

            Assert.AreEqual(item.Timer.Interval, removalTime.TotalMilliseconds);
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

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SettingInactiveJobWhenRunningThrowsException()
        {
            var func = new Mock<IAsyncCommand>();

            func.Setup(f => f.Progress).Returns(new FuncStatus { Status = StatusEnum.Running });

            var container = new Mock<AsyncCommandContainer>(new AsyncCommandItemTimeSpan()) { CallBase = true };

            container.Object.Add(func.Object, "", func.Object.GetType());

            container.Object.SetInactive("", func.Object.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SettingActiveJobWhenNotRunningThrowsException()
        {
            var func = new Mock<IAsyncCommand>();

            func.Setup(f => f.Progress).Returns(new FuncStatus { Status = StatusEnum.Finished });

            var container = new Mock<AsyncCommandContainer>(new AsyncCommandItemTimeSpan()) { CallBase = true };

            container.Object.Add(func.Object, "", func.Object.GetType());

            container.Object.SetActive("", func.Object.GetType());
        }
        #endregion Container

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

            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.Running });

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });

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
            func.Setup(f => f.Progress)
              .Returns(new FuncStatus { Status = StatusEnum.NotStarted });
            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });

            var container = new Mock<IAsyncCommandContainer>();

            AsyncManager a = new AsyncManager(func.Object, "", container.Object);
            
            //act
            a.RunJob();

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
            IAsyncCommand a = new AsyncManager(func.Object, "", container.Object);

            //act
            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });
            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });

            a.Start();

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
            func.Setup(f => f.Progress)
                .Returns(new FuncStatus { Status = StatusEnum.NotStarted });
            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = false });

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

            asyncFunc.RunJob();

            container.Verify(c => c.SetInactive("", func.Object.GetType()));
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
            bool shouldBeTrue = true;

            var func = new Mock<CommandBase>() { CallBase = true };
            var container = new Mock<IAsyncCommandContainer>();
            AsyncManager a = new AsyncManager(func.Object, "", container.Object);

            a.OnSuccess += () => shouldBeTrue = false;

            func.Setup(f => f.CoreStart()).Callback(() => func.Object.Cancel());

            a.RunJob();

            Assert.IsTrue(shouldBeTrue);
        }

        [TestMethod]
        public void OnSuccessCalledWhenNotCancelled()
        {
            bool shouldBeTrue = false;

            var func = new Mock<CommandBase>() { CallBase = true };

            func.Setup(f => f.Properties)
                .Returns(new FuncProperties { ShouldBeRemovedOnComplete = true });

            var container = new Mock<IAsyncCommandContainer>();

            AsyncManager a = new AsyncManager(func.Object, "", container.Object);

            a.OnSuccess += () => shouldBeTrue = true;

            a.RunJob();

            Assert.IsTrue(shouldBeTrue);
        }
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
