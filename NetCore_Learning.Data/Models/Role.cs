using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
