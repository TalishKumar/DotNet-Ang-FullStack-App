using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class AppUser
    {
        // Make sure to use "Id" as name for Entity Framework conventions
        public int Id { get; set; }
        public string UserName { get; set; }
    }
}