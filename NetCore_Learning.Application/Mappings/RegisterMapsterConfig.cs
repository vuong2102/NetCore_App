using Mapster;
using NetCore_Learning.Application.Models.DTO;
using NetCore_Learning.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Application.Mappings
{
    public static class RegisterMapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<UserAccount, AccountDto>.NewConfig()
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.Password, src => src.PasswordHash);

        }
    }
}
