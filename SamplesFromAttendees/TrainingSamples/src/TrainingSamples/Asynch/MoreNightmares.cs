// https://msdn.microsoft.com/en-us/magazine/dn818493.aspx
using System.Threading.Tasks;

namespace TrainingSamples.Asynch
{
    public interface IMyService
    {
        Task<int> GetAsync();
    }
    public sealed class SystemUnderTest
    {
        private readonly IMyService _service;
        public SystemUnderTest(IMyService service)
        {
            _service = service;
        }
        public async Task<int> RetrieveValueAsync()
        {
            return 42 + await _service.GetAsync();
        }
    }
}