using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    internal class LightAnimParser
    {
        public static void Read(ResFileSwitchLoader loader, LightAnim lightAnim)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                lightAnim.Flags = loader.ReadEnum<LightAnimFlags>(true);
                loader.Seek(2);
            }
            else
                loader.LoadHeaderBlock();
            lightAnim.Name = loader.LoadString();
            long CurveArrayOffset = loader.ReadInt64();
            lightAnim.BaseData = loader.LoadCustom(() => new LightAnimData(loader, lightAnim.AnimatedFields));
            lightAnim.UserData = loader.LoadDictValues<UserData>();
            lightAnim.LightTypeName = loader.LoadString();
            lightAnim.DistanceAttnFuncName = loader.LoadString();
            lightAnim.AngleAttnFuncName = loader.LoadString();

            ushort numUserData = 0;
            byte numCurve = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                lightAnim.FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                lightAnim.LightTypeIndex = loader.ReadSByte();
                lightAnim.DistanceAttnFuncIndex = loader.ReadSByte();
                lightAnim.AngleAttnFuncIndex = loader.ReadSByte();
                lightAnim.BakedSize = loader.ReadUInt32();
                numUserData = loader.ReadUInt16();
                loader.Seek(2);
            }
            else
            {
                lightAnim.Flags = loader.ReadEnum<LightAnimFlags>(true);
                numUserData = loader.ReadUInt16();
                lightAnim.FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                lightAnim.LightTypeIndex = loader.ReadSByte();
                lightAnim.DistanceAttnFuncIndex = loader.ReadSByte();
                lightAnim.AngleAttnFuncIndex = loader.ReadSByte();
                lightAnim.BakedSize = loader.ReadUInt32();
            }
            lightAnim.Curves = loader.LoadList<AnimCurve>(numCurve);
        }

        public static void Write(ResFileSwitchSaver saver, LightAnim lightAnim)
        {
            if (saver.ResFile.VersionMajor2 == 9)
            {
                saver.Write(lightAnim.Flags, true);
                saver.Seek(2);
            }
            else
                saver.Seek(12);

            saver.SaveRelocateEntryToSection(saver.Position, 8, 1, 0, ResFileSwitchSaver.Section1, "Light Animation");
            saver.SaveString(lightAnim.Name);
            lightAnim.PosCurveArrayOffset = saver.SaveOffset();
            lightAnim.PosBaseDataOffset = saver.SaveOffset();
            lightAnim.PosUserDataOffset = saver.SaveOffset();
            lightAnim.PosUserDataDictOffset = saver.SaveOffset();
            saver.SaveString(lightAnim.LightTypeName);
            saver.SaveString(lightAnim.DistanceAttnFuncName);
            saver.SaveString(lightAnim.AngleAttnFuncName);
            if (saver.ResFile.VersionMajor2 == 9)
            {
                saver.Write(lightAnim.FrameCount);
                saver.Write((byte)lightAnim.Curves.Count);
                saver.Write(lightAnim.LightTypeIndex);
                saver.Write(lightAnim.DistanceAttnFuncIndex);
                saver.Write(lightAnim.AngleAttnFuncIndex);
                saver.Write(lightAnim.BakedSize);
                saver.Write((ushort)lightAnim.UserData.Count);
                saver.Seek(2);
            }
            else
            {
                saver.Write(lightAnim.Flags, true);
                saver.Write((ushort)lightAnim.UserData.Count);
                saver.Write(lightAnim.FrameCount);
                saver.Write((byte)lightAnim.Curves.Count);
                saver.Write(lightAnim.LightTypeIndex);
                saver.Write(lightAnim.DistanceAttnFuncIndex);
                saver.Write(lightAnim.AngleAttnFuncIndex);
                saver.Write(lightAnim.BakedSize);
            }
        }
    }
}
