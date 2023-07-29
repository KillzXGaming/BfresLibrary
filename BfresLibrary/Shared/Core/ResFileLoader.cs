using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Syroot.Maths;
using Syroot.BinaryData;
using Newtonsoft.Json.Linq;

namespace BfresLibrary.Core
{
    /// <summary>
    /// Loads the hierachy and data of a <see cref="Bfres.ResFile"/>.
    /// </summary>
    public class ResFileLoader : BinaryDataReader
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private IDictionary<uint, IResData> _dataMap;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileLoader"/> class loading data into the given
        /// <paramref name="resFile"/> from the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to load data into.</param>
        /// <param name="stream">The <see cref="Stream"/> to read data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        internal ResFileLoader(ResFile resFile, Stream stream, bool leaveOpen = false)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = resFile;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(IResData resData, ResFile resFile, Stream stream, bool leaveOpen = false)
    : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = resFile;
            _dataMap = new Dictionary<uint, IResData>();
            ImportableFile = resData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileLoader"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to load data into.</param>
        /// <param name="fileName">The name of the file to load the data from.</param>
        internal ResFileLoader(ResFile resFile, string fileName)
            : this(resFile, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(IResData resData, ResFile resFile, string fileName)
    : this(resData, resFile, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        internal bool IsSwitch { get; set; }

        /// <summary>
        /// Gets the loaded <see cref="Bfres.ResFile"/> instance.
        /// </summary>
        internal ResFile ResFile { get; }

        internal IResData ImportableFile { get; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        static internal void ImportSection(string fileName, IResData resData, ResFile resFile) {
            ImportSection(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), resData, resFile);
        }

        static internal void ImportSection(Stream stream, IResData resData, ResFile resFile)
        {
            bool platformSwitch = false;

            using (var reader = new BinaryDataReader(stream, true)) {
                reader.Seek(24, SeekOrigin.Begin);
                platformSwitch = reader.ReadUInt32() != 0;
            }

            if (platformSwitch)
            {
                using (var reader = new Switch.Core.ResFileSwitchLoader(resData, resFile, stream)) {
                    reader.ImportSection();
                }
            }
            else
            {
                using (var reader = new WiiU.Core.ResFileWiiULoader(resData, resFile, stream)) {
                    reader.ImportSection();
                }
            }
        }

        internal void ImportSection()
        {
            ReadImportedFileHeader();

            if (ImportableFile is Shape)
            {
                uint ShapeOffset = ReadUInt32();
                uint VertexBufferOffset = ReadUInt32();

                Seek(ShapeOffset, SeekOrigin.Begin);
                ((IResData)ImportableFile).Load(this);

                Seek(VertexBufferOffset, SeekOrigin.Begin);
                var buffer = new VertexBuffer();
                ((IResData)buffer).Load(this);
                ((Shape)ImportableFile).VertexBuffer = buffer;
            }
            else
            {
                ((IResData)ImportableFile).Load(this);
            }
        }

        private void ReadImportedFileHeader()
        {
            this.ByteOrder = ByteOrder.BigEndian;

            Seek(8, SeekOrigin.Begin); //SUB MAGIC
            ResFile.Version = ReadUInt32();
            ResFile.SetVersionInfo(ResFile.Version);

            string sectionMagic = ReadString(8, Encoding.ASCII);
            uint offset = ReadUInt32();
            uint platformFlag = ReadUInt32();
            ReadUInt32();
            this.ByteOrder = ByteOrder.BigEndian;

            if (platformFlag != 0)
            {
                Seek(0x30, SeekOrigin.Begin);
                this.ByteOrder = ByteOrder.LittleEndian;
            }
        }

        internal ByteOrder ReadByteOrder()
        {
            this.ByteOrder = ByteOrder.BigEndian;
            var bom = ReadEnum<ByteOrder>(true);
            this.ByteOrder = bom;
            return bom;
        }

        /// <summary>
        /// Starts deserializing the data from the <see cref="ResFile"/> root.
        /// </summary>
        internal void Execute()
        {
            // Load the raw data into structures recursively.
            ((IResData)ResFile).Load(this);
        }
        
        /// <summary>
        /// Reads and returns an <see cref="IResData"/> instance of type <typeparamref name="T"/> from the following
        /// offset or returns <c>null</c> if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> to read.</typeparam>
        /// <returns>The <see cref="IResData"/> instance or <c>null</c>.</returns>
        [DebuggerStepThrough]
        internal T Load<T>(bool useOffset = true)
            where T : IResData, new()
        {
            if (!useOffset)
                return ReadResData<T>();

            uint offset = ReadOffset();
            if (offset == 0) return default(T);

            // Seek to the instance data and load it.
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                return ReadResData<T>();
            }
        }

        /// <summary>
        /// Reads and returns an instance of arbitrary type <typeparamref name="T"/> from the following offset with the
        /// given <paramref name="callback"/> or returns <c>null</c> if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the data to read.</typeparam>
        /// <param name="callback">The callback to read the instance data with.</param>
        /// <param name="offset">The optional offset to use instead of reading a following one.</param>
        /// <returns>The data instance or <c>null</c>.</returns>
        /// <remarks>Offset required for ExtFile header (offset specified before size).</remarks>
        [DebuggerStepThrough]
        public virtual T LoadCustom<T>(Func<T> callback, uint? offset = null)
        {
            offset = offset ?? ReadOffset();
            if (offset == 0) return default(T);

            using (TemporarySeek(offset.Value, SeekOrigin.Begin))
            {
                return callback.Invoke();
            }
        }

        /// <summary>
        /// Reads and returns an <see cref="ResDict{T}"/> instance with elements of type <typeparamref name="T"/> from
        /// the following offset or returns an empty instance if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <returns>The <see cref="ResDict{T}"/> instance.</returns>
        [DebuggerStepThrough]
        internal ResDict<T> LoadDict<T>()
            where T : IResData, new()
        {
            uint offset = ReadOffset();
            if (offset == 0) return new ResDict<T>();

            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                ResDict<T> dict = new ResDict<T>();
                ((IResData)dict).Load(this);
                return dict;
            }
        }

        /// <summary>
        /// Reads and returns an <see cref="IList{T}"/> instance with <paramref name="count"/> elements of type
        /// <typeparamref name="T"/> from the following offset or returns <c>null</c> if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <param name="count">The number of elements to expect for the list.</param>
        /// <param name="offset">The optional offset to use instead of reading a following one.</param>
        /// <returns>The <see cref="IList{T}"/> instance or <c>null</c>.</returns>
        /// <remarks>Offset required for FMDL FVTX lists (offset specified before count).</remarks>
        [DebuggerStepThrough]
        internal IList<T> LoadList<T>(int count, uint? offset = null)
            where T : IResData, new()
        {
            List<T> list = new List<T>(count);
            offset = offset ?? ReadOffset();
            if (offset == 0 || count == 0) return new List<T>();

            // Seek to the list start and read it.
            using (TemporarySeek(offset.Value, SeekOrigin.Begin))
            {
                for (; count > 0; count--)
                {
                    list.Add(ReadResData<T>());
                }
                return list;
            }
        }

        /// <summary>
        /// Reads and returns a <see cref="String"/> instance from the following offset or <c>null</c> if the read
        /// offset is 0.
        /// </summary>
        /// <param name="encoding">The optional encoding of the text.</param>
        /// <returns>The read text.</returns>
        [DebuggerStepThrough]
        internal virtual string LoadString(Encoding encoding = null)
        {
            uint offset = ReadOffset();
            if (offset == 0) return null;

            if (StringCache.Strings.ContainsKey(offset))
                return StringCache.Strings[offset];

            encoding = encoding ?? Encoding;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                return ReadString(encoding);
            }
        }

        /// <summary>
        /// Reads and returns <paramref name="count"/> <see cref="String"/> instances from the following offsets.
        /// </summary>
        /// <param name="count">The number of instances to read.</param>
        /// <param name="encoding">The optional encoding of the texts.</param>
        /// <returns>The read texts.</returns>
        [DebuggerStepThrough]
        internal virtual IList<string> LoadStrings(int count, Encoding encoding = null)
        {
            uint[] offsets = ReadOffsets(count);

            encoding = encoding ?? Encoding;
            string[] names = new string[offsets.Length];
            using (TemporarySeek())
            {
                for (int i = 0; i < offsets.Length; i++)
                {
                    uint offset = offsets[i];
                    if (offset == 0) continue;

                    if (StringCache.Strings.ContainsKey(offset))
                        names[i] = StringCache.Strings[offset];
                    else
                    {
                        Position = offset;
                        names[i] = ReadString(encoding);
                    }
                }
                return names;
            }
        }

        /// <summary>
        /// Reads and returns an <see cref="ResDict{T}"/> instance with elements of type <typeparamref name="T"/> from
        /// the following offset or returns an empty instance if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <returns>The <see cref="ResDict{T}"/> instance.</returns>
        [DebuggerStepThrough]
        internal ResDict<T> LoadDictValues<T>()
            where T : IResData, new()
        {
            uint valuesOffset = ReadOffset();
            uint offset = ReadOffset();
            return LoadDictValues<T>(offset, valuesOffset);
        }

        /// <summary>
        /// Reads and returns an <see cref="ResDict{T}"/> instance with elements of type <typeparamref name="T"/> from
        /// the following offset or returns an empty instance if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <returns>The <see cref="ResDict{T}"/> instance.</returns>
        [DebuggerStepThrough]
        internal ResDict<T> LoadDictValues<T>(long dictionaryOffset, long valueOffset)
            where T : IResData, new()
        {
            if (dictionaryOffset == 0) return new ResDict<T>();

            using (TemporarySeek(dictionaryOffset, SeekOrigin.Begin))
            {
                ResDict<T> dict = new ResDict<T>();
                ((IResData)dict).Load(this);

                //Load data via list next to dictionary
                var keys = dict.Keys.ToList();
                var values = LoadList<T>(dict.Count, (uint)valueOffset);

                dict.Clear();
                for (int i = 0; i < keys.Count; i++)
                    dict.Add(keys[i], values[i]);
                return dict;
            }
        }

        public virtual uint ReadSize() => ReadUInt32();

        public virtual string ReadString(Encoding encoding = null) {
            return ReadString(BinaryStringFormat.ZeroTerminated, encoding);
        }

        /// <summary>
        /// Reads a BFRES signature consisting of 4 ASCII characters encoded as an <see cref="UInt32"/> and checks for
        /// validity.
        /// </summary>
        /// <param name="validSignature">A valid signature.</param>
        internal void CheckSignature(string validSignature)
        {
            // Read the actual signature and compare it.
            string signature = ReadString(sizeof(uint), Encoding.ASCII);
            if (signature != validSignature)
            {
                 System.Console.WriteLine($"Invalid signature, expected '{validSignature}' but got '{signature}' at position {Position}.");

                // throw new ResException($"Invalid signature, expected '{validSignature}' but got '{signature}' at position {Position}.");
            }
        }

        /// <summary>
        /// Reads a BFRES offset which is relative to itself, and returns the absolute address.
        /// </summary>
        /// <returns>The absolute address of the offset.</returns>
        public virtual uint ReadOffset()
        {
            uint offset = ReadUInt32();
            return offset == 0 ? 0 : (uint)Position - sizeof(uint) + offset;
        }

        /// <summary>
        /// Reads BFRES offsets which are relative to themselves, and returns the absolute addresses.
        /// </summary>
        /// <param name="count">The number of offsets to read.</param>
        /// <returns>The absolute addresses of the offsets.</returns>
        internal uint[] ReadOffsets(int count)
        {
            uint[] values = new uint[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadOffset();
            }
            return values;
        }

        internal bool[] ReadBit32Booleans(int count)
        {
            bool[] booleans = new bool[count];
            if (count == 0) return booleans;

            int idx = 0;
            while (idx < count)
            {
                uint value = ReadUInt32();
                for (int i = 0; i < 32; i++)
                {
                    if (count <= idx) break;

                    booleans[idx] = (value & 0x1) != 0;
                    value >>= 1;

                    idx++;
                }
            }
            return booleans;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------
        
        [DebuggerStepThrough]
        private T ReadResData<T>()
            where T : IResData, new()
        {
            uint offset = (uint)Position;

            // Same data can be referenced multiple times. Load it in any case to move in the stream, needed for lists.
            T instance = new T();
            instance.Load(this);

            // If possible, return an instance already representing the data.
            if (_dataMap.TryGetValue(offset, out IResData existingInstance))
            {
                return (T)existingInstance;
            }
            else
            {
                _dataMap.Add(offset, instance);
                return instance;
            }
        }
    }
}
