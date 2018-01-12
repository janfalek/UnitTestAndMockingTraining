using System;
using System.Threading.Tasks;

namespace TrainingSamples.Asynch
{
    public class NightmareAsynch
    {
        public async Task SimpleAsync()
        {
            await Task.Delay(2);
//            throw new Exception();
        }
    }
}