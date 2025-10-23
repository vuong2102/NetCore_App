using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class PostTag
    {
        public string PostId { get; set; }
        public Post Post { get; set; } = null!;
        public string TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
