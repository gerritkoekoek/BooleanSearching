using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanSearching
{
    public class Entity
    {
        public virtual bool ContainsSearchQuery(string searchQuery, bool exactMatch = false)
        {
            return false;
        }
    }
}
