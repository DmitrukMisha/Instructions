using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instructions.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public string Text { get; set; }
        public int RecordID { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
    }
}
