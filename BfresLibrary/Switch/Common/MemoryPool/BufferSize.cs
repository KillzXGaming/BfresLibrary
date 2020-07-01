using System.Collections.Generic;
using Syroot.Maths;
using Syroot.NintenTools.Bfres.Core;
using Syroot.NintenTools.Bfres.Switch.Core;

namespace Syroot.NintenTools.Bfres
{
    /// <summary>
    /// Represents a buffer info section
    /// </summary>
    public class BufferSize : IResData
    {
        /// <summary>
        /// the buffer size
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Flag
        /// </summary>
        public uint Flag { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            Size = loader.ReadUInt32();
            Flag = loader.ReadUInt32();
            loader.Seek(40);
        }

        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(Size);
            saver.Write(Flag);
            saver.Seek(8);

            if (((ResFileSwitchSaver)saver).SaveIndexBufferRuntimeData)
                saver.Seek(32);
        }
    }
}
