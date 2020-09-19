using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BfresLibrary.PlatformConverters
{
    internal class MaterialConverterBase
    {
        internal virtual void ConvertToWiiUMaterial(Material material)
        {
        }

        internal virtual void ConvertToSwitchMaterial(Material material)
        {
        }

        internal string RenderInfoBoolString(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
