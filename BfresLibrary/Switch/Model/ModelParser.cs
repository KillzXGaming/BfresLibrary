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
            if (loader.ResFile.VersionMajor2 >= 9)
                model.Flags = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            model.Name = loader.LoadString();
            model.Path = loader.LoadString();
            model.Skeleton = loader.Load<Skeleton>();
            long VertexArrayOffset = loader.ReadOffset();
            model.Shapes = loader.LoadDictValues<Shape>();
            if (loader.ResFile.VersionMajor2 == 9)
            {
                long materialValuesOffset = loader.ReadOffset();
                long materialDictOffset = loader.ReadOffset();
                loader.ReadOffset(); //padding?
                model.Materials = loader.LoadDictValues<Material>(materialDictOffset, materialValuesOffset);
            }
            else
            {
                long materialValuesOffset = loader.ReadOffset();
                long materialDictOffset = loader.ReadOffset();
                model.Materials = loader.LoadDictValues<Material>(materialDictOffset, materialValuesOffset);
            }

            if (loader.ResFile.VersionMajor2 >= 10)
                loader.ReadOffset(); //shader assign offset

            model.UserData = loader.LoadDictValues<UserData>();
            long UserPointer = loader.ReadOffset();
            ushort numVertexBuffer = loader.ReadUInt16();
            ushort numShape = loader.ReadUInt16();
            ushort numMaterial = loader.ReadUInt16();

            ushort numUserData = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                loader.ReadUInt16(); //shader assign count
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
            //Add all shader assign that is bindable
            if (saver.ResFile.VersionMajor2 >= 10)
            {
                foreach (var mat in model.Materials.Values)
                    MaterialParserV10.PrepareSave(mat);

                //Get all the shader assigns
                var list = model.Materials.Values.Select(x => x.ShaderInfoV10.ShaderAssign).Distinct();
                //trim via hash
                model.ShaderAssign = list.GroupBy(x => x.GetHashCode()).Select(g => g.First()).ToList();
                //assign each material shader assign instance
                foreach (var mat in model.Materials.Values) {
                    foreach (var assign in model.ShaderAssign) {
                        if (assign.GetHashCode() == mat.ShaderInfoV10.ShaderAssign.GetHashCode())
                            mat.ShaderInfoV10.ShaderAssign = assign;
                    }
                }
            }

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(model.Flags);
            else
                saver.SaveHeaderBlock();

            if (saver.ResFile.VersionMajor2 >= 10)
                saver.SaveRelocateEntryToSection(saver.Position, 11, 1, 0, ResFileSwitchSaver.Section1, "Model");
            else
                saver.SaveRelocateEntryToSection(saver.Position, 10, 1, 0, ResFileSwitchSaver.Section1, "Model");

            saver.SaveString(model.Name);
            saver.SaveString(model.Path);
            model.SkeletonOffset = saver.SaveOffset();
            model.VertexBufferOffset = saver.SaveOffset();
            model.ShapeOffset = saver.SaveOffset();
            model.ShapeDictOffset = saver.SaveOffset();
            model.MaterialsOffset = saver.SaveOffset();
            model.MaterialsDictOffset = saver.SaveOffset();

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.SaveList(model.ShaderAssign);

            model.PosUserDataOffset = saver.SaveOffset();
            model.PosUserDataDictOffset = saver.SaveOffset();
            saver.Write(0L);
            saver.Write((ushort)model.VertexBuffers.Count);
            saver.Write((ushort)model.Shapes.Count);
            saver.Write((ushort)model.Materials.Count);
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write((ushort)model.ShaderAssign.Count);
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
