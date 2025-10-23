
using System.ComponentModel.DataAnnotations;

namespace NetCore_Learning.Share.Common
{
    public enum RoleEnum
    {
        [Display(Name = "Người dùng")]
        NormalUser = 1,

        [Display(Name = "Quản trị viên")]
        Admin = 2
    }

    public enum ActiveStatusEnum
    {
        Inactive = 0,
        Active = 1
    }
}
