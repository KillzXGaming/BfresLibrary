using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    public class ShapeAnimParser
    {
        internal static void Read(ResFileSwitchLoader loader, ShapeAnim shapeAnim)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
                shapeAnim.Flags = (ShapeAnimFlags)loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            shapeAnim.Name = loader.LoadString();
            shapeAnim.Path = loader.LoadString();
            shapeAnim.BindModel = loader.Load<Model>();
            uint BindIndicesOffset = loader.ReadOffset();
            uint VertexShapeAnimsArrayOffset = loader.ReadOffset();
            shapeAnim.UserData = loader.LoadDictValues<UserData>();
            if (loader.ResFile.VersionMajor2 < 9)
                shapeAnim.Flags = (ShapeAnimFlags)loader.ReadInt16();
            ushort numUserData = loader.ReadUInt16();
            ushort numVertexShapeAnim = loader.ReadUInt16();
            ushort numKeyShapeAnim = loader.ReadUInt16();
            shapeAnim.FrameCount = loader.ReadInt32();
            shapeAnim.BakedSize = loader.ReadUInt32();
            ushort numCurve = loader.ReadUInt16();

            shapeAnim.BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numVertexShapeAnim), BindIndicesOffset);
            shapeAnim.VertexShapeAnims = loader.LoadList<VertexShapeAnim>(numVertexShapeAnim, VertexShapeAnimsArrayOffset);
        }

        public static void Write(ResFileSwitchSaver saver, ShapeAnim shapeAnim)
        {
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write((uint)shapeAnim.Flags);
            else
                saver.SaveHeaderBlock();

            saver.SaveString(shapeAnim.Name);
            saver.SaveString(shapeAnim.Path);
        }
    }
}
