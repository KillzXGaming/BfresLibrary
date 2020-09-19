using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BfresLibrary
{
    interface IBinarySection
    {
        void Import(string filePath, ResFile resFile);
        void Export(string filePath, ResFile resFile);
    }
}
