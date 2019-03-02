using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instructions.Models
{
    public class Like
    {
        public int LikeID { get; set; }
        public Comment CommentID { get; set; }
        public User UserID { get; set; }
    }
}
