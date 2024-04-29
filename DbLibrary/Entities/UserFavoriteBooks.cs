using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.Entities
{
    public class UserFavoriteBooks
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}
