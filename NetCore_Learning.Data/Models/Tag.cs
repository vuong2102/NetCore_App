using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
