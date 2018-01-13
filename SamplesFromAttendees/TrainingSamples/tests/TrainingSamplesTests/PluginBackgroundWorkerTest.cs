using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using TrainingSamples.BackgroundWorker;

namespace TrainingSamplesTests
{
    [TestFixture]
    public class PluginBackgroundWorkerTest
    {
        private bool _wasWorkActionInvoked;
        private bool _wasContinueActionInvoked;
        private CancellationToken _cancellationToken;

        private Mock<IProgress<ProgressMessage>> _progress;
        private Mock<IEventAggregator> _eventAgregator;
        private Mock<IBackgroundWorkerAbort> _backgroundWorkerAbort;
        private PluginBackgroundWorker _backgrounWorker;

        private Action<CancellationToken, IProgress<ProgressMessage>> _workAction;
        private Action<Task, CancellationToken, IProgress<ProgressMessage>> _continueWithAction;

        [SetUp]
        public void SetUp()
        {
            _progress = new Mock<IProgress<ProgressMessage>>();
            _eventAgregator = new Mock<IEventAggregator>();
            _backgroundWorkerAbort = new Mock<IBackgroundWorkerAbort>();
            _backgrounWorker = new PluginBackgroundWorker(_progress.Object, _backgroundWorkerAbort.Object, _eventAgregator.Object);
            
            _workAction = (token, progressObject) =>
            {
                _wasWorkActionInvoked = true;
                _cancellationToken = token;
            };
            _continueWithAction = (task, token, progressObject) =>
            {
                _wasContinueActionInvoked = true;
            };
        }

        [Test]
        public async Task Start_PassOnlyWorkAction_TokenAndProgressArePassed()
        {
            await _backgrounWorker.Start(_workAction);

            Assert.IsTrue(_wasWorkActionInvoked);
            Assert.IsNotNull(_cancellationToken);
        }

        [Test]
        public async Task Start_PassOnlyWorkAction_ReturnProgress()
        {
            _workAction = (token, progressObject) =>
            {
                _wasWorkActionInvoked = true;
                _cancellationToken = token;
                progressObject.Report(new ProgressMessage { MessageType = BackgroundMessageType.InProgress });
            };
            _continueWithAction = (task, token, progressObject) =>
            {
                _wasContinueActionInvoked = true;
                progressObject.Report(new ProgressMessage { MessageType = BackgroundMessageType.InProgress });
            };

            await _backgrounWorker.Start(_workAction, _continueWithAction);

            Assert.IsTrue(_wasWorkActionInvoked);
            Assert.IsNotNull(_cancellationToken);
            _progress.Verify(p => p.Report(It.Is<ProgressMessage>(m => m.MessageType == BackgroundMessageType.InProgress)), Times.Exactly(2));
        }

        [Test]
        public async Task Start_WithCallbackAction_TokenAndProgressArePassed()
        {
            await _backgrounWorker.Start(_workAction, _continueWithAction);

            Assert.IsTrue(_wasWorkActionInvoked);
            Assert.IsTrue(_wasContinueActionInvoked);
            _backgroundWorkerAbort.Verify(bwa => bwa.CancelAbortion(), Times.Once);
            _backgroundWorkerAbort.Verify(bwa => bwa.StartCountingForAbortion(It.IsAny<Thread>()), Times.Never);
        }

        [Test]
        public async Task Start_WithCallbackAction_ProgressFinishedsIsReported()
        {
            await _backgrounWorker.Start(_workAction, _continueWithAction);

            Assert.IsTrue(_wasWorkActionInvoked);
            Assert.IsTrue(_wasContinueActionInvoked);
            _progress.Verify(p => p.Report(It.Is<ProgressMessage>(m => m.MessageType == BackgroundMessageType.Finished)),Times.Once);
        }

        [Test]
        public async Task Start_Cancel_TokenAndProgressArePassed()
        {
            _workAction = (token, progressObject) =>
            {
                _wasWorkActionInvoked = true;
                _cancellationToken = token;
                _backgrounWorker.Handle(null);
            };

            await _backgrounWorker.Start(_workAction, _continueWithAction);

            Assert.IsTrue(_wasWorkActionInvoked);
            Assert.IsFalse(_wasContinueActionInvoked);
            _backgroundWorkerAbort.Verify(bwa => bwa.CancelAbortion(), Times.Once);
            _backgroundWorkerAbort.Verify(bwa => bwa.StartCountingForAbortion(It.IsAny<Thread>()), Times.Once);
        }

    }
}
