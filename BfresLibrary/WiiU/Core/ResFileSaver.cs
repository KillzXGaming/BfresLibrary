using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;
using BfresLibrary.Core;

namespace BfresLibrary.WiiU.Core
{
    /// <summary>
    /// Saves the hierachy and data of a <see cref="Bfres.ResFile"/>.
    /// </summary>
    public class ResFileWiiUSaver : ResFileSaver
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a data block alignment typically seen with <see cref="Buffer.Data"/>.
        /// </summary>
        internal const uint AlignmentSmall = 0x40;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _ofsFileSize;
        private uint _ofsStringPool;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class saving data from the given
        /// <paramref name="resFile"/> into the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to save data from.</param>
        /// <param name="stream">The <see cref="Stream"/> to save data into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        internal ResFileWiiUSaver(ResFile resFile, Stream stream, bool leaveOpen)
    : base(resFile, stream, leaveOpen)
        {
            ByteOrder = ByteOrder.BigEndian;
        }

        internal ResFileWiiUSaver(IResData resData, ResFile resFile, Stream stream, bool leaveOpen)
    : base(resData, resFile, stream, leaveOpen)
        {
            ByteOrder = ByteOrder.BigEndian;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileWiiUSaver(ResFile resFile, string fileName)
    : base(resFile, fileName)
        {
        }

        internal ResFileWiiUSaver(IResData resData, ResFile resFile, string fileName)
    : base(resData, resFile, fileName)
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------


        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> file size field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldFileSize()
        {
            _ofsFileSize = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> string pool size and offset fields which are automatically
        /// filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldStringPool()
        {
            _ofsStringPool = (uint)Position;
            Write(0L);
        }

        // ---- METHODS (PRIVAYE) -------------------------------------------------------------------------------------

        /// <summary>
        /// Starts serializing the data from the <see cref="ResFile"/> root.
        /// </summary>
        public override void Execute()
        {

            // Create queues fetching the names for the string pool and data blocks to store behind the headers.
            _savedItems = new List<ItemEntry>();
            _savedStrings = new SortedDictionary<string, StringEntry>(ResStringComparer.Instance);
            _savedBlocks = new Dictionary<object, BlockEntry>();

            // Store the headers recursively and satisfy offsets to them, then the string pool and data blocks.
            SaveResFie();

            // Satisfy offsets, strings, and data blocks.
            WriteOffsets();
            WriteStrings();
            WriteBlocks();
            // Save final file size into root header at the provided offset.
            Position = _ofsFileSize;
            Write((uint)BaseStream.Length);
            Flush();
        }

        public override void ExportSection()
        {
            if (ExportableData is Model)
            {
                WriteHeader("fresSUB", "FMDL\0\0\0\0");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is Skeleton)
            {
                WriteHeader("fresSUB", "FSKL\0\0\0\0");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is Bone)
            {
                WriteHeader("fmdlSUB", "FSKLBONE");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is Material)
            {
                WriteHeader("fmdlSUB", "FMAT\0\0\0\0");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is Shape)
            {
                WriteHeader("fmdlSUB", "FSHP\0\0\0\0");

                long ShapeOffPos = Position;
                Write(0);
                long VertexBufferOffPos = Position;
                Write(0);

                var offset = Position;
                using (TemporarySeek(ShapeOffPos, SeekOrigin.Begin))
                {
                    Write(offset);
                }
                ((IResData)ExportableData).Save(this);

                offset = Position;
                using (TemporarySeek(VertexBufferOffPos, SeekOrigin.Begin))
                {
                    Write(offset);
                }

                var buffer = ((Shape)ExportableData).VertexBuffer;
                ((IResData)buffer).Save(this);
            }
            else if (ExportableData is SkeletalAnim)
            {
                WriteHeader("fresSUB", "FSKA\0\0\0\0");
                ((IResData)ExportableData).Save(this);

                SaveEntries();
                WriteSkeletonAnimations((SkeletalAnim)ExportableData);
                SaveEntries();
            }
            else if (ExportableData is MaterialAnim)
            {
                WriteHeader("fresSUB", "FTXP\0\0\0\0");
                ((IResData)ExportableData).Save(this);

                SaveEntries();
                WriteMaterialAnimations((MaterialAnim)ExportableData);
                SaveEntries();
            }
            else if (ExportableData is SceneAnim)
            {
                WriteHeader("fresSUB", "FSCN\0\0\0\0");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is CameraAnim)
            {
                WriteHeader("fresSUB", "FSCNFCAM");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is LightAnim)
            {
                WriteHeader("fresSUB", "FSCNFLIT");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is FogAnim)
            {
                WriteHeader("fresSUB", "FSCNFFOG");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is Texture)
            {
                WriteHeader("fresSUB", "FTEX\0\0\0\0");
                ((IResData)ExportableData).Save(this);
            }
            else if (ExportableData is VisibilityAnim)
            {
                WriteHeader("fresSUB", "FVIS\0\0\0\0");
                ((IResData)ExportableData).Save(this);
            }

            WriteEndOfExportData();
        }

        private void WriteStrings()
        {
            // Sort the strings ordinally.
            SortedList<string, StringEntry> sorted = new SortedList<string, StringEntry>(ResStringComparer.Instance);
            foreach (KeyValuePair<string, StringEntry> entry in _savedStrings)
            {
                sorted.Add(entry.Key, entry.Value);
            }

            Align(4);
            uint stringPoolOffset = (uint)Position;

            foreach (KeyValuePair<string, StringEntry> entry in sorted)
            {
                // Align and satisfy offsets.
                Write(entry.Key.Length);
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                // Write the name.
                Write(entry.Key, BinaryStringFormat.ZeroTerminated, entry.Value.Encoding ?? Encoding);
                Align(4);
            }
            BaseStream.SetLength(Position); // Workaround to make last alignment expand the file if nothing follows.

            if (_ofsStringPool != 0)
            {
                // Save string pool offset and size in main file header.
                uint stringPoolSize = (uint)(Position - stringPoolOffset);
                using (TemporarySeek(_ofsStringPool, SeekOrigin.Begin))
                {
                    Write(stringPoolSize);
                    Write((int)(stringPoolOffset - Position));
                }
            }
        }

        private void WriteBlocks()
        {
            foreach (KeyValuePair<object, BlockEntry> entry in _savedBlocks)
            {
                // Align and satisfy offsets.
                if (entry.Value.Alignment != 0) Align((int)entry.Value.Alignment);
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                // Write the data.
                entry.Value.Callback.Invoke();
            }
        }

        private void WriteOffsets()
        {
            using (TemporarySeek())
            {
                foreach (ItemEntry entry in _savedItems)
                {
                    if (entry.Target != null)
                        SatisfyOffsets(entry.Offsets, entry.Target.Value);
                }
            }
        }

        private void SatisfyOffsets(List<uint> offsets, uint target)
        {
            for (int i = 0; i < offsets.Count; i++)
            {
                Position = offsets[i];
                Write((int)(target - offsets[i]));
            }
        }

        private void SaveResFie()
        {
            ((IResData)ResFile).Save(this);

            //These are too slow so we save them manually
            if (ResFile.SkeletalAnims.Count > 0)
            {
                WriteOffset(ResFile.SkeletonAnimationOffset);
                ((IResData)ResFile.SkeletalAnims).Save(this);
            }

            SaveEntries();

            for (int i = 0; i < ResFile.SkeletalAnims.Count; i++)
                WriteSkeletonAnimations(ResFile.SkeletalAnims[i]);
            for (int i = 0; i < ResFile.ShaderParamAnims.Count; i++)
                WriteMaterialAnimations(ResFile.ShaderParamAnims[i]);
            for (int i = 0; i < ResFile.ColorAnims.Count; i++)
                WriteMaterialAnimations(ResFile.ColorAnims[i]);
            for (int i = 0; i < ResFile.TexSrtAnims.Count; i++)
                WriteMaterialAnimations(ResFile.TexSrtAnims[i]);
            for (int i = 0; i < ResFile.TexPatternAnims.Count; i++)
                WriteMaterialAnimations(ResFile.TexPatternAnims[i]);

            SaveEntries();
        }

        private void WriteMaterialAnimations(MaterialAnim anim)
        {
            if (anim.BindIndices?.Length > 0)
            {
                Align(8);
                WriteOffset(anim.PosBindIndicesOffset);
                Write(anim.BindIndices);
            }
            if (anim.MaterialAnimDataList.Count > 0)
            {
                Align(8);
                WriteOffset(anim.PosMatAnimDataOffset);
                for (int i = 0; i < anim.MaterialAnimDataList.Count; i++)
                    anim.MaterialAnimDataList[i].Save(this, anim.signature);
            }
            foreach (MaterialAnimData mat in anim.MaterialAnimDataList)
            {
                if (mat.ParamAnimInfos.Count > 0)
                {
                    Align(4);
                    WriteOffset(mat.PosParamInfoOffset);
                    for (int i = 0; i < mat.ParamAnimInfos.Count; i++)
                        ((IResData)mat.ParamAnimInfos[i]).Save(this);
                }
                if (mat.PatternAnimInfos.Count > 0)
                {
                    Align(4);
                    WriteOffset(mat.PosTexPatInfoOffset);
                    for (int i = 0; i < mat.PatternAnimInfos.Count; i++)
                        ((IResData)mat.PatternAnimInfos[i]).Save(this);
                }
                if (mat.BaseDataList != null && mat.BaseDataList.Length > 0)
                {
                    Align(4);
                    WriteOffset(mat.PosConstantsOffset);
                    this.Write(mat.BaseDataList);
                }
                if (mat.Constants != null && mat.Constants.Count > 0)
                {
                    Align(4);
                    WriteOffset(mat.PosConstantsOffset);
                    this.Write(mat.Constants);
                }
                if (mat.Curves.Count > 0)
                {
                    Align(4);
                    WriteOffset(mat.PosCurvesOffset);
                    for (int i = 0; i < mat.Curves.Count; i++)
                    {
                        mat.Curves[i].SaveEntryBlock = false;
                        ((IResData)mat.Curves[i]).Save(this);
                    }

                    for (int i = 0; i < mat.Curves.Count; i++)
                    {
                        if (mat.Curves[i].Frames.Length > 0)
                        {
                            Align(4);
                            WriteOffset(mat.Curves[i].PosFrameOffset);
                            mat.Curves[i].SaveFrames(this);
                        }
                        if (mat.Curves[i].Keys.Length > 0)
                        {
                            Align(4);
                            WriteOffset(mat.Curves[i].PosKeyDataOffset);
                            mat.Curves[i].SaveKeyData(this);
                        }
                    }
                }
            }
            if (anim.TextureNames?.Count > 0)
            {
                Align(4);
                WriteOffset(anim.PosTextureNamesOffset);
                if (ResFile.Version >= 0x03040002)
                    ((IResData)anim.TextureNames).Save(this);
                else
                {
                    foreach (var texRef in anim.TextureNames.Values)
                        ((IResData)texRef).Save(this);
                }
            }
            if (anim.UserData.Count > 0)
            {
                Align(4);
                WriteOffset(anim.PosUserDataOffset);
                ((IResData)anim.UserData).Save(this);
            }
        }

        private void WriteSkeletonAnimations(SkeletalAnim ska)
        {
            if (ska.BindIndices.Length > 0)
            {
                Align(4);
                WriteOffset(ska.PosBindIndicesOffset);
                Write(ska.BindIndices);
            }
            if (ska.BoneAnims.Count > 0)
            {
                Align(4);
                WriteOffset(ska.PosBoneAnimsOffset);
                for (int i = 0; i < ska.BoneAnims.Count; i++)
                    ((IResData)ska.BoneAnims[i]).Save(this);
            }
            for (int b = 0; b < ska.BoneAnims.Count; b++)
            {
                object baseData = ska.BoneAnims[b].BaseData;
                if (baseData != null && ska.BoneAnims[b].FlagsBase.HasFlag(BoneAnimFlagsBase.Translate) ||
                                        ska.BoneAnims[b].FlagsBase.HasFlag(BoneAnimFlagsBase.Rotate) ||
                                        ska.BoneAnims[b].FlagsBase.HasFlag(BoneAnimFlagsBase.Scale))
                {
                    Align(4);
                    WriteOffset(ska.BoneAnims[b].PosBaseDataOffset);
                    ska.BoneAnims[b].BaseData.Save(this, ska.BoneAnims[b].FlagsBase);
                }

                if (ska.BoneAnims[b].Curves.Count > 0)
                {
                    Align(4);
                    WriteOffset(ska.BoneAnims[b].PosCurvesOffset);
                    for (int i = 0; i < ska.BoneAnims[b].Curves.Count; i++)
                    {
                        ska.BoneAnims[b].Curves[i].SaveEntryBlock = false;
                        ((IResData)ska.BoneAnims[b].Curves[i]).Save(this);
                    }

                    for (int i = 0; i < ska.BoneAnims[b].Curves.Count; i++)
                    {
                        if (ska.BoneAnims[b].Curves[i].Frames.Length > 0)
                        {
                            Align(4);
                            WriteOffset(ska.BoneAnims[b].Curves[i].PosFrameOffset);
                            ska.BoneAnims[b].Curves[i].SaveFrames(this);
                        }
                        if (ska.BoneAnims[b].Curves[i].Keys.Length > 0)
                        {
                            Align(4);
                            WriteOffset(ska.BoneAnims[b].Curves[i].PosKeyDataOffset);
                            ska.BoneAnims[b].Curves[i].SaveKeyData(this);
                        }
                    }
                }
            }
            if (ska.UserData.Count > 0)
            {
                WriteOffset(ska.PosUserDataOffset);
                ((IResData)ska.UserData).Save(this);
                Align(8);
            }
        }

        private void SaveEntries()
        {
            // Store all queued items. Iterate via index as subsequent calls append to the list.
            for (int i = 0; i < _savedItems.Count; i++)
            {
                if (_savedItems[i].Target != null)
                {
                    // Ignore if it has already been written (list or dict elements).
                    continue;
                }

                ItemEntry entry = _savedItems[i];

                Align(4);
                switch (entry.Type)
                {
                    case ItemEntryType.List:
                        IEnumerable<IResData> list = (IEnumerable<IResData>)entry.Data;
                        // Check if the first item has already been written by a previous dict.
                        if (TryGetItemEntry(list.First(), ItemEntryType.ResData, out ItemEntry firstElement))
                        {
                            entry.Target = firstElement.Target;
                        }
                        else
                        {
                            entry.Target = (uint)Position;
                            CurrentIndex = 0;
                            foreach (IResData element in list)
                            {
                                _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, target: (uint)Position,
                                    index: CurrentIndex));
                                element.Save(this);
                                CurrentIndex++;
                            }
                        }
                        break;

                    case ItemEntryType.Dict:
                    case ItemEntryType.ResData:
                        entry.Target = (uint)Position;
                        CurrentIndex = entry.Index;
                        ((IResData)entry.Data).Save(this);
                        break;

                    case ItemEntryType.Custom:
                        entry.Target = (uint)Position;
                        entry.Callback.Invoke();
                        break;
                }
            }
        }


        private bool TryGetItemEntry(object data, ItemEntryType type, out ItemEntry entry)
        {
            foreach (ItemEntry savedItem in _savedItems)
            {
                if (savedItem.Data.Equals(data) && savedItem.Type == type)
                {
                    entry = savedItem;
                    return true;
                }
            }
            entry = null;
            return false;
        }
    }
}
