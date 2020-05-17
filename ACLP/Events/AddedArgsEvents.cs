using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Morpher.Events
{
    public class AddedArgsEvents : EventArgs
    {
        public object Arg { get; set; }
    }
}
