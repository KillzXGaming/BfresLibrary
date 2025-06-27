using BfresLibrary.Core;

namespace BfresLibrary
{
    public class KeyShape : IResData
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public byte[] TargetAttribIndices { get; set; }

        public byte[] TargetAttribIndexOffsets { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            TargetAttribIndices = loader.ReadBytes(20);
            TargetAttribIndexOffsets = loader.ReadBytes(4);
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.Write(TargetAttribIndices);
            saver.Write(TargetAttribIndexOffsets);
        }
    }
}