using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Core
{
    using Microsoft.EntityFrameworkCore.Storage;
    using NetCore_Learning.Data.Models;
    using System;
    using System.Threading.Tasks;

    namespace YourApp.Core.Interfaces
    {
        public interface IUnitOfWork : IDisposable
        {
            Task<int> CommitAsync();

            Task<IDbContextTransaction> BeginTransactionAsync();

            Task RollbackTransactionAsync();

            Task CommitTransactionAsync();
        }
    }

}
