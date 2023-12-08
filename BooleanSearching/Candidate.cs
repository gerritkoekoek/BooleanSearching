using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BooleanSearching
{
    public class Candidate : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Town {  get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }

        public override bool ContainsSearchQuery(string searchQuery, bool exactMatch = false)
        {
            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    string value = (string)property.GetValue(this, null); 

                    if (value != null)
                    {
                        if (exactMatch && value.Equals(searchQuery, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        if (!exactMatch && value.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override string ToString()
        {
            return $"{FirstName} {LastName} {Town} {PhoneNumber} {Description}";
        }
    }
}
