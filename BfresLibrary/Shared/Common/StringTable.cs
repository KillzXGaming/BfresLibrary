using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary.Core;
using Syroot.BinaryData;

namespace BfresLibrary
{
    public class StringTable : IResData
    {
        public List<string> Strings = new List<string>();

        void IResData.Load(ResFileLoader loader)
        {
            Strings.Clear();
            if (loader.IsSwitch)
            {
                loader.Seek(-0x14, System.IO.SeekOrigin.Current);
                uint Signature = loader.ReadUInt32();
                uint blockOffset = loader.ReadUInt32();
                long BlockSize = loader.ReadInt64();
                uint StringCount = loader.ReadUInt32();

                for (int i = 0; i < StringCount + 1; i++)
                {
                    ushort size = loader.ReadUInt16();
                    Strings.Add(loader.ReadString(BinaryStringFormat.ZeroTerminated));
                    loader.Align(2);
                }
            }
        }
        void IResData.Save(ResFileSaver saver)
        {

        }
    }
}
