using BfresLibrary.Core;
using System.Diagnostics;

namespace BfresLibrary
{
    public class KeyShape : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public byte[] TargetAttribIndices { get; set; }

        public KeyShape()
        {
            TargetAttribIndices = new byte[18];
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            TargetAttribIndices = loader.ReadBytes(18);
            loader.ReadUInt16(); // padding
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(TargetAttribIndices);
            saver.Write((ushort)0);
        }
    }
}