using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    internal class CameraAnimParser
    {
        public static void Read(ResFileSwitchLoader loader, CameraAnim cameraAnim)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                cameraAnim.Flags = loader.ReadEnum<CameraAnimFlags>(true);
                loader.Seek(2);
            }
            else
                loader.LoadHeaderBlock();
            cameraAnim.Name = loader.LoadString();
            long CurveArrayOffset = loader.ReadInt64();
            cameraAnim.BaseData = loader.LoadCustom(() => new CameraAnimData(loader));
            cameraAnim.UserData = loader.LoadDictValues<UserData>();

            byte numCurve = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                cameraAnim.FrameCount = loader.ReadInt32();
                cameraAnim.BakedSize = loader.ReadUInt32();
                ushort numUserData = loader.ReadUInt16();
                numCurve = loader.ReadByte();
                loader.Seek(5);
            }
            else
            {
                cameraAnim.Flags = loader.ReadEnum<CameraAnimFlags>(true);
                loader.Seek(2);
                cameraAnim.FrameCount = loader.ReadInt32();
                numCurve = loader.ReadByte();
                loader.Seek(1);
                ushort numUserData = loader.ReadUInt16();
                cameraAnim.BakedSize = loader.ReadUInt32();
            }

            cameraAnim.Curves = loader.LoadList<AnimCurve>(numCurve, (uint)CurveArrayOffset);
        }

        public static void Write(ResFileSwitchSaver saver, CameraAnim cameraAnim)
        {
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write(cameraAnim.Flags, true);
                saver.Seek(2);
            }
            else
                saver.Seek(12);

            saver.SaveRelocateEntryToSection(saver.Position, 5, 1, 0, ResFileSwitchSaver.Section1, "Camera Animation");
            saver.SaveString(cameraAnim.Name);
            cameraAnim.PosCurveArrayOffset = saver.SaveOffset();
            cameraAnim.PosBaseDataOffset = saver.SaveOffset();
            cameraAnim.PosUserDataOffset = saver.SaveOffset();
            cameraAnim.PosUserDataDictOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 == 9)
            {
                saver.Write(cameraAnim.FrameCount);
                saver.Write(cameraAnim.BakedSize);
                saver.Write((ushort)cameraAnim.UserData.Count);
                saver.Write((byte)cameraAnim.Curves.Count);
                saver.Seek(5);
            }
            else
            {
                saver.Write(cameraAnim.Flags, true);
                saver.Seek(2);
                saver.Write(cameraAnim.FrameCount);
                saver.Write((byte)cameraAnim.Curves.Count);
                saver.Seek(1);
                saver.Write((ushort)cameraAnim.UserData.Count);
                saver.Write(cameraAnim.BakedSize);
            }
        }
    }
}
