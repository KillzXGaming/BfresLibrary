using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    internal class FogAnimParser
    {
        public static void Read(ResFileSwitchLoader loader, FogAnim fogAnim)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                fogAnim.Flags = loader.ReadEnum<FogAnimFlags>(true);
                loader.Seek(2);
            }
            else
                loader.LoadHeaderBlock();
            fogAnim.Name = loader.LoadString();
            long CurveArrayOffset = loader.ReadInt64();
            fogAnim.BaseData = loader.LoadCustom(() => new FogAnimData(loader));
            fogAnim.UserData = loader.LoadDictValues<UserData>();
            fogAnim.DistanceAttnFuncName = loader.LoadString();

            ushort numUserData = 0;
            byte numCurve = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                fogAnim.FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                fogAnim.DistanceAttnFuncIndex = loader.ReadSByte();
                numUserData = loader.ReadUInt16();
                fogAnim.BakedSize = loader.ReadUInt32();
                loader.Seek(4);
            }
            else
            {
                fogAnim.Flags = loader.ReadEnum<FogAnimFlags>(true);
                loader.Seek(2);
                fogAnim.FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                fogAnim.DistanceAttnFuncIndex = loader.ReadSByte();
                numUserData = loader.ReadUInt16();
                fogAnim.BakedSize = loader.ReadUInt32();
            }

            fogAnim.Curves = loader.LoadList<AnimCurve>(numCurve);
        }

        public static void Write(ResFileSwitchSaver saver, FogAnim fogAnim)
        {
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(fogAnim.Flags, true);
                saver.Seek(2);
            }
            else
                saver.Seek(12);

            saver.SaveRelocateEntryToSection(saver.Position, 6, 1, 0, ResFileSwitchSaver.Section1, "Fog Animation");
            saver.SaveString(fogAnim.Name);
            fogAnim.PosCurveArrayOffset = saver.SaveOffset();
            fogAnim.PosBaseDataOffset = saver.SaveOffset();
            fogAnim.PosUserDataOffset = saver.SaveOffset();
            fogAnim.PosUserDataDictOffset = saver.SaveOffset();
            saver.SaveString(fogAnim.DistanceAttnFuncName);

            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(fogAnim.FrameCount);
                saver.Write((byte)fogAnim.Curves.Count);
                saver.Write(fogAnim.DistanceAttnFuncIndex);
                saver.Write((ushort)fogAnim.UserData.Count);
                saver.Write(fogAnim.BakedSize);
                saver.Seek(4);
            }
            else
            {
                saver.Write(fogAnim.Flags, true);
                saver.Seek(2);
                saver.Write(fogAnim.FrameCount);
                saver.Write((byte)fogAnim.Curves.Count);
                saver.Write(fogAnim.DistanceAttnFuncIndex);
                saver.Write((ushort)fogAnim.UserData.Count);
                saver.Write(fogAnim.BakedSize);
            }
        }
    }
}
