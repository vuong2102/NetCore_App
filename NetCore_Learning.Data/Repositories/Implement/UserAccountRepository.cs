using Microsoft.EntityFrameworkCore;
using NetCore_Learning.Data.Configuration;
using NetCore_Learning.Data.Core;
using NetCore_Learning.Data.Models;
using NetCore_Learning.Data.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Repositories.Implement
{
    public class UserAccountRepository : GenericRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
