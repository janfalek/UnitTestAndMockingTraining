using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using TrainingSamples.BackgroundWorker;

namespace TrainingSamplesTests
{
    using System.Threading;

    [TestFixture]
    public class BackgroundWorkerTests
    {
        private Mock<IProgress<ProgressMessage>> progressMock;
        private Mock<IEventAggregator> eventAgregator;
        private Mock<IBackgroundWorkerAbort> backgroungWorkerAbortMock;
        private PluginBackgroundWorker backgrounWorker;

        [SetUp]
        public void Setup()
        {
            progressMock = new Mock<IProgress<ProgressMessage>>();
            eventAgregator = new Mock<IEventAggregator>();
            backgroungWorkerAbortMock = new Mock<IBackgroundWorkerAbort>();
            backgrounWorker = new PluginBackgroundWorker(progressMock.Object, backgroungWorkerAbortMock.Object, eventAgregator.Object);
        }

        [Test]
        public async Task Start_WhenCalled_ShouldInvokeBothMethods()
        {
            var workActionInvoked = false;
            var continueWithActionInvoked = false;
            var workAction = new Action<CancellationToken, IProgress<ProgressMessage>>((token, progress) =>
                    workActionInvoked = true
                );
            var continueWith = new Action<Task, CancellationToken, IProgress<ProgressMessage>>((task, token, progress) =>
                    continueWithActionInvoked = true
                );

            await backgrounWorker.Start(workAction, continueWith);

            Assert.IsTrue(workActionInvoked);
            Assert.IsTrue(continueWithActionInvoked);
            backgroungWorkerAbortMock.Verify(x => x.CancelAbortion(), Times.Once);
            backgroungWorkerAbortMock.Verify(x => x.StartCountingForAbortion(It.IsAny<Thread>()), Times.Never);
            progressMock.Verify(x => x.Report(It.IsAny<ProgressMessage>()), Times.Once);
        }

        [Test]
        public async Task Start_HandleCancel_DoNotInvokeContinueWithMethod()
        {
            var workActionInvoked = false;
            var continueWithActionInvoked = false;
            var workAction = new Action<CancellationToken, IProgress<ProgressMessage>>((token, progress) =>
                {
                    workActionInvoked = true;
                    backgrounWorker.Handle(null);
                });
            var continueWith = new Action<Task, CancellationToken, IProgress<ProgressMessage>>((task, token, progress) =>
                continueWithActionInvoked = true
            );

            await backgrounWorker.Start(workAction, continueWith);

            Assert.IsTrue(workActionInvoked);
            Assert.IsFalse(continueWithActionInvoked);
            progressMock.Verify(x => x.Report(It.IsAny<ProgressMessage>()), Times.Once);
        }

        [Test]
        public async Task Start_Exception_SouldExecuteContinueWithMethod()
        {
            var continueWithActionInvoked = false;
            var workAction = new Action<CancellationToken, IProgress<ProgressMessage>>((token, progress) => throw new NotImplementedException());
            var continueWith = new Action<Task, CancellationToken, IProgress<ProgressMessage>>((task, token, progress) =>
                continueWithActionInvoked = true
            );

            await backgrounWorker.Start(workAction, continueWith);

            Assert.IsTrue(continueWithActionInvoked);
            backgroungWorkerAbortMock.Verify(x => x.CancelAbortion(), Times.Once);
            progressMock.Verify(x => x.Report(It.IsAny<ProgressMessage>()), Times.Once);
        }
    }
}
