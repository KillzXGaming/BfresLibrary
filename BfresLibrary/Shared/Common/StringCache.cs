using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BfresLibrary
{
    /// <summary>
    /// A cache of strings that are saved through raw IDs in place of the pointer field of a name offset.
    /// This is used and required for TOTK models.
    /// </summary>
    public class StringCache
    {
        public static Dictionary<long, string> Strings = new Dictionary<long, string>();
    }
}
