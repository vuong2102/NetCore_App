using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Data.Models
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; } = null!;

        public string PostId { get; set; }
        public Post Post { get; set; } = null!;

        public string AuthorId { get; set; }
        public User Author { get; set; } = null!;

        public string? ParentId { get; set; }
        public Comment? Parent { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }

}
