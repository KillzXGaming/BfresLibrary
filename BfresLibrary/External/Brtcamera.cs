using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syroot.BinaryData;

namespace BfresLibrary
{
    /// <summary>
    /// 
    /// </summary>
    public class Brtcamera
    {
        /// <summary>
        /// 
        /// </summary>
        public char[] Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint FrameCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public KeyFrame[] KeyFrames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float UnknownValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBigEndian = true;

        /// <summary>
        /// 
        /// </summary>
        public Brtcamera(Stream stream, bool bigEndian) {
            IsBigEndian = bigEndian;
            using (var reader = new BinaryDataReader(stream)) {
                Read(reader);
            }
        }

        public void Save(Stream stream)
        {
            using (var writer = new BinaryDataWriter(stream)) {
                Write(writer);
            }
        }

        private void Read(BinaryDataReader reader)
        {
            reader.ByteOrder = ByteOrder.LittleEndian;
            if (IsBigEndian)
                reader.ByteOrder = ByteOrder.BigEndian;

            Name = reader.ReadChars(64);
            uint count = reader.ReadUInt32();
            FrameCount = reader.ReadUInt32();
            UnknownValue = reader.ReadSingle();

            KeyFrames = new KeyFrame[count];
            for (int i = 0; i < count; i++)
            {
                KeyFrames[i] = new KeyFrame()
                {
                    Flag = reader.ReadUInt32(),
                    Frame = reader.ReadSingle(),
                    Data = reader.ReadSingles(8),
                };
            }
        }

        private void Write(BinaryDataWriter writer)
        {
            writer.ByteOrder = ByteOrder.LittleEndian;
            if (IsBigEndian)
                writer.ByteOrder = ByteOrder.BigEndian;
            writer.Write(Name);
            writer.Write(KeyFrames.Length);
            writer.Write(FrameCount);
            writer.Write(UnknownValue);
            for (int i = 0; i < KeyFrames.Length; i++)
            {
                writer.Write(KeyFrames[i].Flag);
                writer.Write(KeyFrames[i].Frame);
                writer.Write(KeyFrames[i].Data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class KeyFrame
        {
            /// <summary>
            /// 
            /// </summary>
            public uint Flag;

            /// <summary>
            /// 
            /// </summary>
            public float Frame;

            /// <summary>
            /// 
            /// </summary>
            public float[] Data = new float[8];
        }
    }
}
