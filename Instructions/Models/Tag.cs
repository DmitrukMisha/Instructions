﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instructions.Models
{
    public class Tag
    {
        public int TagID { get; set; }
        public Record Record { get; set; }
        public string TagName { get; set; }
    }
}
