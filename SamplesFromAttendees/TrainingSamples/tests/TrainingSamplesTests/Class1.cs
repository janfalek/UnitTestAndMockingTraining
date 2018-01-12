using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TrainingSamples.BackgroundWorker;

namespace TrainingSamplesTests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void BackgroundTest()
        {
            var progress = new Mock<IProgress<ProgressMessage>>();
            var eventAgregator = new Mock<IEventAggregator>();
            var backgrounWorker =
                new PluginBackgroundWorker(progress.Object, new BackgroundWorkerAbort(), eventAgregator.Object);

        }
    }
}
