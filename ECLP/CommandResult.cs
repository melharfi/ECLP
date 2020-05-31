using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Morpher
{
    public class CommandResult
    {
        /// <summary>
        /// Type 1) Args are arguments could be any referenced primitif type like string, int, bool, float, and char ex "hello 5 true 3,5 T".
        /// Argument is parsed to it's appropriate type, otherwize it return the default value as string
        /// Make attention that positions of arguments is necessary so you can identify them in the Args list by there index.
        /// </summary>
        public List<object> Args = new List<object>();

        /// <summary>
        /// Type 2) Flags are string type arguments without values, prefixed with double dash -- "--verbose --start --friendlyfire".
        /// Value is parsed to appropriate type, otherwize the default format is 'string'
        /// </summary>
        public List<string> Flags = new List<string>();

        /// <summary>
        /// Type 3) Properties are a property with value prefixed with -p "-p driver=steave -p age=30".
        /// Value is parsed to appropriate type, otherwize the default format is 'string'
        /// </summary>
        public Dictionary<string, object> Properties = new Dictionary<string, object>();

        /// <summary>
        /// Type 4) Collections is a property with a collection of values separated by Pipe |.
        /// Should be prefixed with -c "-c players=steave|john|clark -c ages=21|15|30" players is the name of the property, and steave,john,clark are a list of object values.
        /// </summary>
        public Dictionary<string, object[]> Collections = new Dictionary<string, object[]>();

        /// <summary>
        /// Type 5) ExCollections for Extanded Collections, it's a property with a collection of sub properties that have a value.
        /// Should be prefixed with -xc "-xc players=steave:21|john:15|clark:30 -xc adresses=Japan:Tokyo|USA:Washington".
        /// Properties are separated by Pipe |, and sub property name and it's values are separated by double point : .
        /// </summary>
        public Dictionary<string, List<KeyValuePair<string, object>>> ExCollections = new Dictionary<string, List<KeyValuePair<string, object>>>();

        public void Clear()
        {
            Args.Clear();
            Collections.Clear();
            ExCollections.Clear();
            Flags.Clear();
            Properties.Clear();
        }
    }
}
