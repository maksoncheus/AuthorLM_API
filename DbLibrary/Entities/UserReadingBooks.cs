﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.Entities
{
    public class UserReadingBooks
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        [ForeignKey(nameof(BookId))]
        public virtual Book Book { get; set; }
    }
}
