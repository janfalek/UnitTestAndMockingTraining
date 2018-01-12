using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TrainingSamples.Asynch;

namespace TrainingSamplesTests.Asynch
{
    [TestFixture]
    public class NightmareAsynchTests1
    {
        //        [Test]
        //        public async void SimpleAsynch_Playing_DontKnow()
        //        {
        //            var nightmare = new NightmareAsynch();
        //
        //            try
        //            {
        //                await nightmare.SimpleAsync();
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine(e);
        //                throw;
        //            }
        //        }

        [Test]
        public async Task RetrieveValue_SynchronousSuccess_Adds42()
        {
            var service = new Mock<IMyService>();
            service.Setup(x => x.GetAsync()).Returns(() => Task.FromResult(5));
            // Or: service.Setup(x => x.GetAsync()).ReturnsAsync(5);
            var system = new SystemUnderTest(service.Object);
            var result = await system.RetrieveValueAsync();
            Assert.AreEqual(47, result);
        }
        [Test]
        public async Task RetrieveValue_AsynchronousSuccess_Adds42()
        {
            var service = new Mock<IMyService>();
            service.Setup(x => x.GetAsync()).Returns(async () =>
            {
                await Task.Yield();
                return 5;
            });
            var system = new SystemUnderTest(service.Object);
            var result = await system.RetrieveValueAsync();
            Assert.AreEqual(47, result);
        }
        [Test]
        public void RetrieveValue_AsynchronousFailure_Throws()
        {
            var service = new Mock<IMyService>();
            service.Setup(x => x.GetAsync()).Returns(async () =>
            {
                await Task.Yield();
                throw new Exception();
            });
            var system = new SystemUnderTest(service.Object);
            Assert.ThrowsAsync<Exception>(async () => await system.RetrieveValueAsync());
//            var exception = Assert.ThrowsAsync<Exception>(async () => await system.RetrieveValueAsync()));

            //https://github.com/nunit/docs/wiki/Assert.ThrowsAsync
        }
    }
}