using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Morpher.Events
{
    public class AddedExCollectionEventArgs : EventArgs
    {
        public KeyValuePair<string, List<KeyValuePair<string, object>>> ExCollections;

    }
}
