using System;
using System.Collections.Generic;

namespace RolesFunction
{
    public class Role
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public List<string> PrivilegeTypes { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime UpdatedTimestamp { get; set; }
    }
}
