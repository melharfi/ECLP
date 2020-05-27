using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Morpher.Events
{
    public class AddedPropertiesEvents : EventArgs
    {
        public KeyValuePair<string, object> Property;
    }
}
