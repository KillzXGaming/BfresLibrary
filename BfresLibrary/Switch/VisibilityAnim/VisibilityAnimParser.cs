using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;
using System.Runtime.CompilerServices;

namespace BfresLibrary.Switch
{
    internal class VisibilityAnimParser
    {
        public static void Read(ResFileSwitchLoader loader, VisibilityAnim visibilityAnim)
        {
            loader.CheckSignature("FBVS");
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                visibilityAnim._flags = loader.ReadUInt16();
                loader.ReadUInt16(); //Padding
            }
            else
                loader.LoadHeaderBlock();
            visibilityAnim.Name = loader.LoadString();
            visibilityAnim.Path = loader.LoadString();
            visibilityAnim.BindModel = loader.Load<Model>();
            long BindIndicesOffset = loader.ReadInt64();
            long CurveArrayOffset = loader.ReadInt64();
            long BaseDataArrayOffset = loader.ReadInt64();
            long NameArrayOffset = loader.ReadInt64();
            visibilityAnim.UserData = loader.LoadDictValues<UserData>();

            if (loader.ResFile.VersionMajor2 < 9)
                visibilityAnim._flags = loader.ReadUInt16();
            else
                loader.ReadUInt16(); //Idk what this is

            ushort numUserData = loader.ReadUInt16();
            visibilityAnim.FrameCount = loader.ReadInt32();
            ushort numAnim = loader.ReadUInt16();
            ushort numCurve = loader.ReadUInt16();
            visibilityAnim.BakedSize = loader.ReadUInt32();

            visibilityAnim.BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numAnim), (uint)BindIndicesOffset);
            visibilityAnim.Names = loader.LoadCustom(() => loader.LoadStrings(numAnim), (uint)NameArrayOffset); // Offset to name list.
            visibilityAnim.Curves = loader.LoadList<AnimCurve>(numCurve, (uint)CurveArrayOffset);

            visibilityAnim.BaseDataList = loader.LoadCustom(() =>
            {
                return loader.ReadBit32Booleans(numAnim);
            }, (uint)BaseDataArrayOffset);
        }

        public static void Write(ResFileSwitchSaver saver, VisibilityAnim visibilityAnim)
        {
            saver.WriteSignature("FBVS");
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(visibilityAnim._flags);
                saver.Write((ushort)0);
            }
            else
                saver.SaveHeaderBlock();

            saver.SaveString(visibilityAnim.Name);
            saver.SaveString(visibilityAnim.Path);
            saver.Write(0L); //Bind Model
            visibilityAnim.PosBindIndicesOffset = saver.SaveOffset();
            visibilityAnim.PosCurvesOffset = saver.SaveOffset();
            visibilityAnim.PosBaseDataOffset = saver.SaveOffset();
            visibilityAnim.PosNamesOffset = saver.SaveOffset();
            visibilityAnim.PosUserDataOffset = saver.SaveOffset();
            visibilityAnim.PosUserDataDictOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 < 9)
                saver.Write(visibilityAnim._flags);
            else
                saver.Write((ushort)0);
            saver.Write((ushort)visibilityAnim.UserData.Count);
            saver.Write(visibilityAnim.FrameCount);
            if (visibilityAnim.Names != null)
                saver.Write((ushort)visibilityAnim.Names.Count);
            else
                saver.Write((ushort)0);
            saver.Write((ushort)visibilityAnim.Curves.Count);
            saver.Write(visibilityAnim.BakedSize);
        }
    }
}
