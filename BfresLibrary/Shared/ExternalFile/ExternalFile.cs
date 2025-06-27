using System.IO;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a file attachment to a <see cref="ResFile"/> which can be of arbitrary data.
    /// </summary>
    public class ExternalFile : IResData, INamed
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the raw data stored by the external file.
        /// </summary>
        public byte[] Data { get; set; }
        public string Name { get; set; }

        public object LoadedFileData { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Opens and returns a <see cref="MemoryStream"/> on the raw <see cref="Data"/> byte array, which optionally
        /// can be written to.
        /// </summary>
        /// <param name="writable"><c>true</c> to allow write access to the raw data.</param>
        /// <returns>The opened <see cref="MemoryStream"/> instance.</returns>
        public MemoryStream GetStream(bool writable = false)
        {
            return new MemoryStream(Data, writable);
        }

        void IResData.Load(ResFileLoader loader)
        {
            uint ofsData = loader.ReadOffset();
            uint sizData = loader.ReadSize();
            Data = loader.LoadCustom(() => loader.ReadBytes((int)sizData), ofsData);
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                ((Switch.Core.ResFileSwitchSaver)saver).SaveRelocateEntryToSection(saver.Position, 1, 1, 0,
                    Switch.Core.ResFileSwitchSaver.Section5, "External files");
            }

            if (Data ==  null)
                Data = new byte[0];

            if (Data.Length <= 3)
                saver.SaveBlock(Data, (int)512, () => saver.Write(Data));
            else
                saver.SaveBlock(Data, (uint)saver.ResFile.DataAlignment, () => saver.Write(Data));
            saver.WriteSize((uint)Data.Length);
        }
    }
}