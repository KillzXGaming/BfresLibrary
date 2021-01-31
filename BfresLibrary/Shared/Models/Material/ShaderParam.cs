using System;
using System.Diagnostics;
using BfresLibrary.Core;

namespace BfresLibrary
{
    /// <summary>
    /// Represents a parameter value in a <see cref="UserData"/> section, passing data to shader variables.
    /// </summary>
    [DebuggerDisplay(nameof(ShaderParam) + " {" + nameof(Name) + "}")]
    [Serializable]
    public class ShaderParam : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderParam"/> class.
        /// </summary>
        public ShaderParam()
        {
            Name = "";
            Type = ShaderParamType.Float;
            DataOffset = 0;
            DependedIndex = 0;
            DependIndex = 0;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the type of the value.
        /// </summary>
        public ShaderParamType Type { get; set; }

        /// <summary>
        /// Gets the offset in the <see cref="Material.ShaderParamData"/> byte array in bytes.
        /// </summary>
        public ushort DataOffset { get; set; }

        public ushort DependedIndex { get; set; }

        public ushort DependIndex { get; set; }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{ShaderParam}"/> instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the size of the value in bytes.
        /// </summary>
        public uint DataSize
        {
            get
            {
                if (Type <= ShaderParamType.Float4)
                {
                    return sizeof(float) * (((uint)Type & 0x03) + 1);
                }
                if (Type <= ShaderParamType.Float4x4)
                {
                    uint cols = ((uint)Type & 0x03) + 1;
                    uint rows = (((uint)Type - (uint)ShaderParamType.Reserved2) >> 2) + 2;
                    return sizeof(float) * cols * rows;
                }
                switch (Type)
                {
                    case ShaderParamType.Srt2D: return Srt2D.SizeInBytes;
                    case ShaderParamType.Srt3D: return Srt3D.SizeInBytes;
                    case ShaderParamType.TexSrt: return TexSrt.SizeInBytes;
                    case ShaderParamType.TexSrtEx: return TexSrt.SizeInBytes + 4;
                }
                throw new ResException($"Cannot retrieve size of unknown {nameof(ShaderParamType)} {Type}.");
            }
        }

        public object DataValue { get; set; }

        public uint callbackPointer { get; set; }
        public int offset { get; set; }

        public bool UsePadding { get; set; }
        public int PaddingLength { get; set; }

        // ---- METHODS ------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.IsSwitch)
            {
                long callbackPointer = loader.ReadInt64();
                Name = loader.LoadString();
                Type = loader.ReadEnum<ShaderParamType>(true);
                byte sizData = loader.ReadByte();
                DataOffset = loader.ReadUInt16();
                int offset = loader.ReadInt32(); // Uniform variable offset.
                DependedIndex = loader.ReadUInt16();
                DependIndex = loader.ReadUInt16();
                uint padding2 = loader.ReadUInt32(); // Uniform variable offset.
            }
            else
            {
                Type = loader.ReadEnum<ShaderParamType>(true);
                byte sizData = loader.ReadByte();

                if (sizData != (byte)DataSize && sizData > DataSize)
                {
                    UsePadding = true;
                    PaddingLength = sizData - (byte)DataSize;
                }

                DataOffset = loader.ReadUInt16();
                offset = loader.ReadInt32(); // Uniform variable offset.
                if (loader.ResFile.Version >= 0x03040000)
                {
                    callbackPointer = loader.ReadUInt32();
                    DependedIndex = loader.ReadUInt16();
                    DependIndex = loader.ReadUInt16();
                }
                else if (loader.ResFile.Version >= 0x03030000
                    && loader.ResFile.Version < 0x03040000)
                {
                    callbackPointer = loader.ReadUInt32();
                    DependedIndex = loader.ReadUInt16();
                    DependIndex = loader.ReadUInt16();
                    uint FMATOffset = loader.ReadUInt32(); //Why does this have this????
                }
                Name = loader.LoadString();
            }
        }

        void IResData.Save(ResFileSaver saver)
        {
            if (saver.IsSwitch)
            {
                saver.Write((long)0); // CallbackPointer
                saver.SaveString(Name);
                saver.Write(Type, true);
                saver.Write((byte)DataSize);
                saver.Write(DataOffset);
                saver.Write(-1); // Offset
                saver.Write(DependedIndex);
                saver.Write(DependIndex);
                saver.Write(0);// Uniform variable offset.
            }
            else
            {
                saver.Write(Type, true);
                if (saver.ResFile.Version >= 0x03040000)
                    saver.Write((byte)(DataSize + PaddingLength));
                else
                    saver.Write((byte)0);

                saver.Write(DataOffset);
                saver.Write(offset); // Offset
                if (saver.ResFile.Version >= 0x03030000)
                {
                    saver.Write(callbackPointer); // CallbackPointer
                    saver.Write(DependedIndex);
                    saver.Write(DependIndex);
                }
                else if (saver.ResFile.Version >= 0x03030000
                    && saver.ResFile.Version < 0x03040000)
                {
                    saver.Write(callbackPointer); // CallbackPointer
                    saver.Write(DependedIndex);
                    saver.Write(DependIndex);
                    saver.Write(0);
                }

                saver.SaveString(Name);
            }
        }
    }

    /// <summary>
    /// Represents the data types in which <see cref="ShaderParam"/> instances can store their value.
    /// </summary>
    [Serializable]
    public enum ShaderParamType : byte
    {
        /// <summary>
        /// The value is a single <see cref="System.Boolean"/>.
        /// </summary>
        Bool,

        /// <summary>
        /// The value is a <see cref="Maths.Vector2Bool"/>.
        /// </summary>
        Bool2,

        /// <summary>
        /// The value is a <see cref="Maths.Vector3Bool"/>.
        /// </summary>
        Bool3,

        /// <summary>
        /// The value is a <see cref="Maths.Vector4Bool"/>.
        /// </summary>
        Bool4,

        /// <summary>
        /// The value is a single <see cref="System.Int32"/>.
        /// </summary>
        Int,

        /// <summary>
        /// The value is a <see cref="Maths.Vector2"/>.
        /// </summary>
        Int2,

        /// <summary>
        /// The value is a <see cref="Maths.Vector3"/>.
        /// </summary>
        Int3,

        /// <summary>
        /// The value is a <see cref="Maths.Vector4"/>.
        /// </summary>
        Int4,

        /// <summary>
        /// The value is a single <see cref="System.UInt32"/>.
        /// </summary>
        UInt,

        /// <summary>
        /// The value is a <see cref="Maths.Vector2U"/>.
        /// </summary>
        UInt2,

        /// <summary>
        /// The value is a <see cref="Maths.Vector3U"/>.
        /// </summary>
        UInt3,

        /// <summary>
        /// The value is a <see cref="Maths.Vector4U"/>.
        /// </summary>
        UInt4,

        /// <summary>
        /// The value is a single <see cref="System.Single"/>.
        /// </summary>
        Float,

        /// <summary>
        /// The value is a <see cref="Maths.Vector2F"/>.
        /// </summary>
        Float2,

        /// <summary>
        /// The value is a <see cref="Maths.Vector3F"/>.
        /// </summary>
        Float3,

        /// <summary>
        /// The value is a <see cref="Maths.Vector4F"/>.
        /// </summary>
        Float4,

        /// <summary>
        /// An invalid type for <see cref="ShaderParam"/> values, only used for internal computations.
        /// </summary>
        Reserved2,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix2"/>.
        /// </summary>
        Float2x2,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix2x3"/>.
        /// </summary>
        Float2x3,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix2x4"/>.
        /// </summary>
        Float2x4,

        /// <summary>
        /// An invalid type for <see cref="ShaderParam"/> values, only used for internal computations.
        /// </summary>
        Reserved3,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix3x2"/>.
        /// </summary>
        Float3x2,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix3"/>.
        /// </summary>
        Float3x3,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix3x4"/>.
        /// </summary>
        Float3x4,

        /// <summary>
        /// An invalid type for <see cref="ShaderParam"/> values, only used for internal computations.
        /// </summary>
        Reserved4,

        /// <summary>
        /// The value is a <see cref="System.Single"/>.
        /// </summary>
        Float4x2,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix4x3"/>.
        /// </summary>
        Float4x3,

        /// <summary>
        /// The value is a <see cref="Maths.Matrix4"/>.
        /// </summary>
        Float4x4,

        /// <summary>
        /// The value is a <see cref="Srt2D"/>.
        /// </summary>
        Srt2D,

        /// <summary>
        /// The value is a <see cref="Srt3D"/>.
        /// </summary>
        Srt3D,

        /// <summary>
        /// The value is a <see cref="TexSrt"/>.
        /// </summary>
        TexSrt,

        /// <summary>
        /// The value is a <see cref="TexSrtEx"/>.
        /// </summary>
        TexSrtEx
    }
}