using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BfresLibrary.Switch.Core;
using BfresLibrary.Core;
using BfresLibrary;

namespace BfresLibrary.Switch
{
    public class SceneAnimParser
    {
        internal static void Read(ResFileSwitchLoader loader, SceneAnim sceneAnim)
        {
            if (loader.ResFile.VersionMajor2 >= 9)
                sceneAnim.Flags = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            sceneAnim.Name = loader.LoadString();
            sceneAnim.Path = loader.LoadString();
            sceneAnim.CameraAnims = loader.LoadDictValues<CameraAnim>();
            sceneAnim.LightAnims = loader.LoadDictValues<LightAnim>();
            sceneAnim.FogAnims = loader.LoadDictValues<FogAnim>();
            sceneAnim.UserData = loader.LoadDictValues<UserData>();
            ushort numUserData = loader.ReadUInt16();
            ushort numCameraAnim = loader.ReadUInt16();
            ushort numLightAnim = loader.ReadUInt16();
            ushort numFogAnim = loader.ReadUInt16();
        }

        public static void Write(ResFileSwitchSaver saver, SceneAnim sceneAnim)
        {
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(sceneAnim.Flags);
            else
                saver.SaveHeaderBlock();

            saver.SaveString(sceneAnim.Name);
            saver.SaveString(sceneAnim.Path);
            sceneAnim.PosCameraAnimArrayOffset = saver.SaveOffset();
            sceneAnim.PosCameraAnimDictOffset = saver.SaveOffset();
            sceneAnim.PosLightAnimArrayOffset = saver.SaveOffset();
            sceneAnim.PosLightAnimDictOffset = saver.SaveOffset();
            sceneAnim.PosFogAnimArrayOffset = saver.SaveOffset();
            sceneAnim.PosFogAnimDictOffset = saver.SaveOffset();
            sceneAnim.PosUserDataOffset = saver.SaveOffset();
            sceneAnim.PosUserDataDictOffset = saver.SaveOffset();
            saver.Write((ushort)sceneAnim.UserData.Count);
            saver.Write((ushort)sceneAnim.CameraAnims.Count);
            saver.Write((ushort)sceneAnim.LightAnims.Count);
            saver.Write((ushort)sceneAnim.FogAnims.Count);
        }
    }
}
