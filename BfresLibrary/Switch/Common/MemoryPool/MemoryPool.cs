using System.Collections.Generic;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a buffer info section
    /// </summary>
    public class MemoryPool : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int Size = 288;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        public byte[] PoolArray { get; set; }

        void IResData.Load(ResFileLoader loader)
        {
           // PoolArray = loader.ReadBytes(Size);
        }

        void IResData.Save(ResFileSaver saver)
        {
         //   saver.Write(PoolArray);
        }
    }
}
