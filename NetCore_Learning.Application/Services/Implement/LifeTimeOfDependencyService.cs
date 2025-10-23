using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Application.Services.Interface;

namespace NetCore_Learning.Application.Services.Implement
{
    public class LifeTimeOfDependencyService : ILifeTimeOfDependencyService
    {
        private readonly string _operationId;

        public LifeTimeOfDependencyService()
        {
            _operationId = Guid.NewGuid().ToString();
        }
        public string GetOperationId() => _operationId;

        public async Task<string> LoginRequestAsync(AccountDto account)
        {
            await Task.Delay(10);
            return _operationId;
        }
    }
}
