using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    public class ModelParser
    {
        internal static void Read(ResFileSwitchLoader loader, Model model)
        {
            if (loader.ResFile.VersionMajor2 == 9)
                model.Flags = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            model.Name = loader.LoadString();
            model.Path = loader.LoadString();
            model.Skeleton = loader.Load<Skeleton>();
            long VertexArrayOffset = loader.ReadOffset();
            model.Shapes = loader.LoadDictValues<Shape>();
            long materialValuesOffset = loader.ReadOffset();
            if (loader.ResFile.VersionMajor2 == 9)
                loader.ReadOffset(); //padding?

            long materialDictOffset = loader.ReadOffset();
            model.Materials = loader.LoadDictValues<Material>(materialDictOffset, materialValuesOffset);
            model.UserData = loader.LoadDictValues<UserData>();
            long UserPointer = loader.ReadOffset();
            ushort numVertexBuffer = loader.ReadUInt16();
            ushort numShape = loader.ReadUInt16();
            ushort numMaterial = loader.ReadUInt16();

            ushort numUserData = 0;
            if (loader.ResFile.VersionMajor2 == 9)
            {
                loader.ReadUInt16(); //padding?
                numUserData = loader.ReadUInt16();
                loader.ReadUInt16(); //padding?
                uint padding = loader.ReadUInt32();
            }
            else
            {
                numUserData = loader.ReadUInt16();
                uint totalVertexCount = loader.ReadUInt32();
                uint padding = loader.ReadUInt32();
            }

            model.VertexBuffers = loader.LoadList<VertexBuffer>(numVertexBuffer, (uint)VertexArrayOffset);
        }

        public static void Write(ResFileSwitchSaver saver, Model model)
        {
            if (saver.ResFile.VersionMajor2 == 9)
                saver.Write(model.Flags);
            else
                saver.SaveHeaderBlock();

            saver.SaveRelocateEntryToSection(saver.Position, 10, 1, 0, ResFileSwitchSaver.Section1, "Model");
            saver.SaveString(model.Name);
            saver.SaveString(model.Path);
            model.SkeletonOffset = saver.SaveOffset();
            model.VertexBufferOffset = saver.SaveOffset();
            model.ShapeOffset = saver.SaveOffset();
            model.ShapeDictOffset = saver.SaveOffset();
            model.MaterialsOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 == 9)
                saver.Write(0L);
            model.MaterialsDictOffset = saver.SaveOffset();
            model.PosUserDataOffset = saver.SaveOffset();
            model.PosUserDataDictOffset = saver.SaveOffset();
            saver.Write(0L);
            saver.Write((ushort)model.VertexBuffers.Count);
            saver.Write((ushort)model.Shapes.Count);
            saver.Write((ushort)model.Materials.Count);
            if (saver.ResFile.VersionMajor2 == 9)
            {
                saver.Write((ushort)0);
                saver.Write((ushort)model.UserData.Count);
                saver.Write((ushort)0);
                saver.Write(0);
            }
            else
            {
                saver.Write((ushort)model.UserData.Count);
                saver.Write(model.TotalVertexCount);
                saver.Write(0);
            }
        }
    }
}
