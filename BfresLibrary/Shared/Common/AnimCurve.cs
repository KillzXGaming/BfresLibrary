using BfresLibrary.Core;
using System.ComponentModel;
using System.Collections.Generic;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an animation curve used by several sections to control different parameters over time.
    /// </summary>
    public class AnimCurve : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimCurve"/> class.
        /// </summary>
        public AnimCurve()
        {
            FrameType = AnimCurveFrameType.Single;
            KeyType = AnimCurveKeyType.Single;
            CurveType = AnimCurveType.Cubic;

            AnimDataOffset = 0;
            StartFrame = 0;
            EndFrame = 0;
            Scale = 0;
            Frames = new float[0];
            KeyStepBoolData = new bool[0];
            Keys = new float[0, 4];
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const ushort _flagsMaskFrameType = 0b00000000_00000011;
        private const ushort _flagsMaskKeyType = 0b00000000_00001100;
        private const ushort _flagsMaskCurveType = 0b00000000_01110000;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private ushort _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the data type in which <see cref="Frames"/> are loaded and saved. For simplicity, the class
        /// always stores frames as converted <see cref="System.Single"/> instances.
        /// </summary>
        public AnimCurveFrameType FrameType
        {
            get { return (AnimCurveFrameType)(_flags & _flagsMaskFrameType); }
            set { _flags = (ushort)(_flags & ~_flagsMaskFrameType | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets the data type in which <see cref="Keys"/> are loaded and saved. For simplicity, the class
        /// always stores frames as converted <see cref="System.Single"/> instances.
        /// </summary>
        public AnimCurveKeyType KeyType
        {
            get { return (AnimCurveKeyType)(_flags & _flagsMaskKeyType); }
            set { _flags = (ushort)(_flags & ~_flagsMaskKeyType | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets the curve type, determining the number of elements stored with each key.
        /// </summary>
        public AnimCurveType CurveType
        {
            get { return (AnimCurveType)(_flags & _flagsMaskCurveType); }
            set { _flags = (ushort)(_flags & ~_flagsMaskCurveType | (ushort)value); }
        }

        /// <summary>
        /// Gets or sets the pre wrap mode, determining how to wrap the key data.
        /// </summary>
        public WrapMode PreWrap
        {
            get { return (WrapMode)(_flags >> 8 & 3); }
            set { _flags = (ushort)(_flags & 64767 | (ushort)value << 8); }
        }

        /// <summary>
        /// Gets or sets the post wrap mode, determining how to wrap the key data.
        /// </summary>
        public WrapMode PostWrap
        {
            get { return (WrapMode)(_flags >> 12 & 3); }
            set { _flags = (ushort)(_flags & 53247 | (ushort)value << 12); }
        }

        /// <summary>
        /// Gets or sets the memory offset relative to the start of the corresponding animation data structure to
        /// animate the field stored at that address. Note that enums exist in the specific animation which map offsets
        /// to names.
        /// </summary>
        public uint AnimDataOffset { get; set; }

        /// <summary>
        /// Gets or sets the first frame at which a key is placed.
        /// </summary>
        public float StartFrame { get; set; }

        /// <summary>
        /// Gets or sets the last frame at which a key is placed.
        /// </summary>
        public float EndFrame { get; set; }

        /// <summary>
        /// Gets or sets the scale to multiply values of the curve by.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// Gets or sets the offset to add to the values of the curve (after multiplicating them).
        /// </summary>
        public DWord Offset { get; set; }

        /// <summary>
        /// Gets or sets the difference between the lowest and highest key value.
        /// </summary>
        public float Delta { get; set; }

        /// <summary>
        /// Gets the frame numbers at which keys of the same index in the <see cref="Keys"/> array are placed.
        /// </summary>
        public float[] Frames { get; set; }

        /// <summary>
        /// Gets an array of elements forming the elements of keys placed at the frames of the same index in the
        /// <see cref="Frames"/> array.
        /// </summary>
        public float[,] Keys { get; set; }

        public bool[] KeyStepBoolData { get; set; }

        private int ElementsPerKey
        {
            get
            {
                switch (CurveType)
                {
                    case AnimCurveType.Cubic:
                        return 4;
                    case AnimCurveType.Linear:
                        return 2;
                    default:
                        return 1;
                }
            }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calculates the bake size of the animation curve.
        /// </summary>
        /// <returns></returns>
        public uint CalculateBakeSize(bool isSwitch)
        {
            if (isSwitch)
            {
                if (CurveType == AnimCurveType.StepInt || CurveType == AnimCurveType.StepBool)
                    return (uint)(1 * (EndFrame - StartFrame + 1) + 12) + 4;
                else
                    return (uint)(4 * (EndFrame - StartFrame + 1) + 32 ) - 4;
            }
            else
            {
                if (CurveType == AnimCurveType.StepInt || CurveType == AnimCurveType.StepBool)
                    return (uint)(1 * (EndFrame - StartFrame + 1) + 8);
                else
                    return (uint)(4 * (EndFrame - StartFrame + 1) + 20);
            }
        }

        public AnimCurve Copy()
        {
            AnimCurve curve = new AnimCurve();
            curve._flags = this._flags;
            curve.AnimDataOffset = this.AnimDataOffset;
            curve.CurveType = this.CurveType;
            curve.FrameType = this.FrameType;
            curve.Delta = this.Delta;
            curve.EndFrame = this.EndFrame;
            curve.StartFrame = this.StartFrame;
            curve.Offset = this.Offset;
            curve.Scale = this.Scale;
            curve.Frames = new float[this.Frames.Length];
            curve.Keys = new float[this.Keys.Length / this.ElementsPerKey, this.ElementsPerKey];

            for (int i = 0; i < this.Frames.Length; i++)
                curve.Frames[i] = this.Frames[i];

            for (int i = 0; i < this.Keys.Length / this.ElementsPerKey; i++)
            {
                for (int j = 0; j < this.ElementsPerKey; j++)
                    curve.Keys[i, j] = this.Keys[i, j];
            }

            return curve;
        }

        void IResData.Load(ResFileLoader loader)
        {
            long FrameArrayOffset = 0;
            long KeyArrayOffset = 0;
            ushort numKey = 0;
            if (loader.IsSwitch) {
                FrameArrayOffset = loader.ReadOffset();
                KeyArrayOffset = loader.ReadOffset();
                _flags = loader.ReadUInt16();
                numKey = loader.ReadUInt16();
                AnimDataOffset = loader.ReadUInt32();
                StartFrame = loader.ReadSingle();
                EndFrame = loader.ReadSingle();
                Scale = loader.ReadSingle();
                Offset = loader.ReadSingle();
                Delta = loader.ReadSingle();
                int padding = loader.ReadInt32();
            }
            else
            {
                _flags = loader.ReadUInt16();
                numKey = loader.ReadUInt16();
                AnimDataOffset = loader.ReadUInt32();
                StartFrame = loader.ReadSingle();
                EndFrame = loader.ReadSingle();
                Scale = loader.ReadSingle();
                Offset = loader.ReadSingle();
                if (loader.ResFile.Version >= 0x03040000)
                {
                    Delta = loader.ReadSingle();
                }
                FrameArrayOffset = loader.ReadOffset();
                KeyArrayOffset = loader.ReadOffset();
            }

            Frames = loader.LoadCustom(() =>
            {
                switch (FrameType)
                {
                    case AnimCurveFrameType.Single:
                        return loader.ReadSingles(numKey);
                    case AnimCurveFrameType.Decimal10x5:
                        float[] dec10x5Frames = new float[numKey];
                        for (int i = 0; i < numKey; i++)
                        {
                            dec10x5Frames[i] = (float)loader.ReadDecimal10x5();
                        }
                        return dec10x5Frames;
                    case AnimCurveFrameType.Byte:
                        float[] byteFrames = new float[numKey];
                        for (int i = 0; i < numKey; i++)
                        {
                            byteFrames[i] = loader.ReadByte();
                        }
                        return byteFrames;
                    default:
                        throw new ResException($"Invalid {nameof(FrameType)}.");
                }
            }, (uint)FrameArrayOffset);
            Keys = loader.LoadCustom(() =>
            {
                int elementsPerKey = ElementsPerKey;
                float[,] keys = new float[numKey, elementsPerKey];
                switch (KeyType)
                {
                    case AnimCurveKeyType.Single:
                        for (int i = 0; i < numKey; i++)
                        {
                            for (int j = 0; j < elementsPerKey; j++)
                            {
                                if (CurveType == AnimCurveType.StepInt || CurveType == AnimCurveType.StepBool)
                                    keys[i, j] = loader.ReadUInt32();
                                else
                                    keys[i, j] = loader.ReadSingle();
                            }
                        }
                        break;
                    case AnimCurveKeyType.Int16:
                        for (int i = 0; i < numKey; i++)
                        {
                            for (int j = 0; j < elementsPerKey; j++)
                            {
                                keys[i, j] = loader.ReadInt16();
                            }
                        }
                        break;
                    case AnimCurveKeyType.SByte:
                        for (int i = 0; i < numKey; i++)
                        {
                            for (int j = 0; j < elementsPerKey; j++)
                            {
                                keys[i, j] = loader.ReadSByte();
                            }
                        }
                        break;
                    default:
                        throw new ResException($"Invalid {nameof(KeyType)}.");
                }
                return keys;
            }, (uint)KeyArrayOffset);

            //Bools use bits to store values. 
            if (CurveType == AnimCurveType.StepBool)
            {
                int keyIndex = 0;

                KeyStepBoolData = new bool[numKey];
                for (int i = 0; i < Keys.Length; i++) {
                    if (numKey <= keyIndex) break;

                    int value = (int)Keys[i, 0];

                    //Bit shift each key value
                    for (int j = 0; j < 32; j++)
                    {
                        if (numKey <= keyIndex) break;

                        bool set = (value & 0x1) != 0;
                        value >>= 1;

                        KeyStepBoolData[keyIndex] = set;
                        keyIndex++;
                    }
                }
            }
        }

        internal long PosFrameOffset;
        internal long PosKeyDataOffset;
        internal bool SaveEntryBlock = true;

        void IResData.Save(ResFileSaver saver)
        {
            UpdateBooleanKeys();

            if (saver.IsSwitch)
            {
                PosFrameOffset = saver.SaveOffset();
                PosKeyDataOffset = saver.SaveOffset();
                saver.Write(_flags);
                saver.Write((ushort)Frames.Length);
                saver.Write(AnimDataOffset);
                saver.Write(StartFrame);
                saver.Write(EndFrame);
                saver.Write(Scale);
                saver.Write(Offset);
                saver.Write(Delta);
                saver.Write(0);
            }
            else
            {
                saver.Write(_flags);
                saver.Write((ushort)Frames.Length);
                saver.Write(AnimDataOffset);
                saver.Write(StartFrame);
                saver.Write(EndFrame);
                saver.Write(Scale);
                saver.Write(Offset);
                if (saver.ResFile.Version >= 0x03040000)
                {
                    saver.Write(Delta);
                }

                if (SaveEntryBlock)
                {
                    saver.SaveCustom(Frames, () =>
                    {
                        SaveFrames(saver);
                    });
                    saver.SaveCustom(Keys, () =>
                    {
                        SaveKeyData(saver);
                    });
                }
                else
                {
                    PosFrameOffset = saver.SaveOffsetPos();
                    PosKeyDataOffset = saver.SaveOffsetPos();
                }
            }
        }

        private void UpdateBooleanKeys()
        {
            if (CurveType != AnimCurveType.StepBool)
                return;

            int bitPosition = 0;
            var keyData = KeyStepBoolData;
            //32 boolean bits per key
            List<uint> keys = new List<uint>() { 0 };

            for (int i = 0; i < keyData.Length; i++) {
                //Set bit for keyed data
                if (keyData[i])
                    keys[keys.Count - 1] |= (uint)(1 << bitPosition);

                bitPosition++;
                //Reset position and add a new key after 32 bits
                //Make sure to skip adding a new key if this is the last boolean key
                if (bitPosition > 32 && keyData.Length - 1 != i) {
                    keys.Add(0);
                    bitPosition = 0;
                }
            }
            //Apply the key data
            this.Keys = new float[keys.Count, 1];
            for (int i = 0; i < keys.Count; i++)
                this.Keys[i, 0] = keys[i];
        }

        public void SaveFrames(ResFileSaver saver)
        {
            switch (FrameType)
            {
                case AnimCurveFrameType.Single:
                    saver.Write(Frames);
                    break;
                case AnimCurveFrameType.Decimal10x5:
                    foreach (float frame in Frames)
                    {
                        saver.Write((Decimal10x5)frame);
                    }
                    break;
                case AnimCurveFrameType.Byte:
                    foreach (float frame in Frames)
                    {
                        saver.Write((byte)frame);
                    }
                    break;
            }
        }

        public void SaveKeyData(ResFileSaver saver)
        {
            switch (KeyType)
            {
                case AnimCurveKeyType.Single:
                    foreach (float key in Keys)
                    {
                        if (CurveType == AnimCurveType.StepInt || CurveType == AnimCurveType.StepBool)
                            saver.Write((uint)key);
                        else
                            saver.Write(key);
                    }
                    break;
                case AnimCurveKeyType.Int16:
                    foreach (float key in Keys)
                    {
                        saver.Write((short)key);
                    }
                    break;
                case AnimCurveKeyType.SByte:
                    foreach (float key in Keys)
                    {
                        saver.Write((sbyte)key);
                    }
                    break;
            }
        }
    }



    /// <summary>
    /// Represents the possible data types in which <see cref="AnimCurve.Frames"/> are stored. For simple library use,
    /// they are always converted them to and from <see cref="Single"/> instances.
    /// </summary>
    public enum AnimCurveFrameType : ushort
    {
        /// <summary>
        /// The frames are stored as <see cref="System.Single"/> instances.
        /// </summary>
        Single,

        /// <summary>
        /// The frames are stored as <see cref="Bfres.Decimal10x5"/> instances.
        /// </summary>
        Decimal10x5,

        /// <summary>
        /// The frames are stored as <see cref="System.Byte"/> instances.
        /// </summary>
        Byte
    }

    /// <summary>
    /// Represents the possible data types in which <see cref="AnimCurve.Keys"/> are stored. For simple library use,
    /// they are always converted them to and from <see cref="Single"/> instances.
    /// </summary>
    public enum AnimCurveKeyType : ushort
    {
        /// <summary>
        /// The keys are stored as <see cref="System.Single"/> instances.
        /// </summary>
        Single = 0 << 2,

        /// <summary>
        /// The keys are stored as <see cref="Bfres.Decimal10x5"/> instances.
        /// </summary>
        Int16 = 1 << 2,

        /// <summary>
        /// The keys are stored as <see cref="System.SByte"/> instances.
        /// </summary>
        SByte = 2 << 2
    }

    /// <summary>
    /// Represents the type of key values stored by this curve. This also determines the number of required elements to
    /// define a key in the <see cref="AnimCurve.Keys"/> array. Use the <see cref="AnimCurve.ElementsPerKey()"/>
    /// method to retrieve the number of elements required for the <see cref="AnimCurve.CurveType"/> of that curve.
    /// </summary>
    public enum AnimCurveType : ushort
    {
        /// <summary>
        /// The curve uses cubic interpolation. 4 elements of the <see cref="AnimCurve.Keys"/> array form a key.
        /// </summary>
        Cubic = 0 << 4,

        /// <summary>
        /// The curve uses linear interpolation. 2 elements of the <see cref="AnimCurve.Keys"/> array form a key.
        /// </summary>
        Linear = 1 << 4,

        /// <summary>
        /// 1 element of the <see cref="AnimCurve.Keys"/> array forms a key.
        /// </summary>
        BakedFloat = 2 << 4,

        /// <summary>
        /// 1 element of the <see cref="AnimCurve.Keys"/> array forms a key.
        /// </summary>
        StepInt = 4 << 4,

        /// <summary>
        /// 1 element of the <see cref="AnimCurve.Keys"/> array forms a key.
        /// </summary>
        BakedInt = 5 << 4,

        /// <summary>
        /// 1 element of the <see cref="AnimCurve.Keys"/> array forms a key.
        /// </summary>
        StepBool = 6 << 4,

        /// <summary>
        /// 1 element of the <see cref="AnimCurve.Keys"/> array forms a key.
        /// </summary>
        BakedBool = 7 << 4
    }

    public enum WrapMode
    {
        Clamp,
        Repeat,
        Mirror,
    }

    public struct AnimConstant
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the memory offset relative to the start of the corresponding animation data structure to
        /// animate the field stored at that address. Note that enums exist in the specific animation which map offsets
        /// to names.
        /// </summary>
        public uint AnimDataOffset;

        public DWord Value;
    }
}