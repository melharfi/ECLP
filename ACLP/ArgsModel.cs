using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Morpher
{
    public class ArgsModel
    {
        /// <summary>
        /// Type 1) Args are arguments could be any referenced primitif type like string, int, bool, float, dateTime, char and decimal ex "verbose 5 3,5 true --v".
        /// Argument is parsed to it's appropriate type, otherwize it return the default value as string
        /// Make attention that positions of arguments is necessary so you can identify them in the Args list by there index.
        /// </summary>
        public List<object> Args = new List<object>();

        /// <summary>
        /// Type 2) Flags are string type arguments without values, prefixed with -f "-f Driver -f Car -f California".
        /// Another way to define many flags without define prefix multi time is by using -f-m prefix and separate values by Pipe | , ex "-f-m driver|car|california" is same as "-f Driver -f Car -f California"
        /// Value is parsed to appropriate type, otherwize the default format is 'string'
        /// </summary>
        public List<string> Flags = new List<string>();

        /// <summary>
        /// Type 3) Properties are a property with value prefixed with -p "-p driver=steave -p age=30".
        /// Another way to define many properties without define prefix multi time is by using -p-m and separate values by Pipe | , ex "-p-m driver=steave|age=30" is equivalent to "-p driver=steave -p age=30", Result is not merged into one object but separated as you expected
        /// Value is parsed to appropriate type, otherwize the default format is 'string'
        /// </summary>
        public Dictionary<string, object> Properties = new Dictionary<string, object>();

        /// <summary>
        /// Type 4) Collections is a property with a collection of values separated by Pipe |.
        /// Should be prefixed with -c "-c players=steave|john|clark -c ages=21|15|30" players is the name of the property, and steave,john,clark are a list of object values.
        /// Another way to define many values without define -c prefix multi time is by using -c-m and separate values by double point :, ex "-c-m players=steave|john|clark/ages=21|15|30" is equivalent to "-c players=steave|john|clark -c ages=21|15|30", Result is not merged into one object but separated as you expected
        /// </summary>
        public Dictionary<string, object[]> Collections = new Dictionary<string, object[]>();

        /// <summary>
        /// Type 5) ExCollections for Extanded Collections, it's a property with a collection of sub properties that have a value.
        /// Should be prefixed with -xc "-xc players=steave:21|john:15|clark:30 -xc adresses=Japan:Tokyo|USA:Washington".
        /// Properties are separated by Pipe |, and sub property name and it's values are separated by double point : .
        /// Can't define multi ExCollections cause data will contain many parameters separated with many char which make confusion
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
