using System;
using System.IO;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.TextConvert;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ResFile animFile = new ResFile("JugemObj.bfres");
            animFile.Save("JugemObjRB.bfres");
            return;

            ResFile resFile = new ResFile(args[0]);

            string folder = Path.GetFileNameWithoutExtension(args[0]);
            foreach (var model in resFile.Models.Values)
            {
                if (!Directory.Exists($"{folder}/Models/{model.Name}"))
                    Directory.CreateDirectory($"{folder}/Models/{model.Name}");

                foreach (var mat in model.Materials.Values) {
                    File.WriteAllText($"{folder}/Models/{model.Name}/{mat.Name}.json", MaterialConvert.ToJson(mat));
                }
            }
/*
            string targetFileWiiU = "test.bfres";


            ResFile resFile = new ResFile(targetFileWiiU);
            resFile.Models[0].Export("ModelU.bfmdl", resFile);
            resFile.Save($"{targetFileWiiU}.new.bfres");

            //Platform switch, alignment, version
            resFile.ChangePlatform(true, 4096, 0, 5, 0, 3);
            resFile.Save("newNX.bfres");

            //Section export/import test
            //Swap a switch model with a wii u one
            resFile.Models[0].Export("ModelNX.bfmdl", resFile);
            resFile.Models[0].Import("ModelU.bfmdl", resFile);

            //Platform wii u, alignment, version
            resFile.ChangePlatform(false, 8192, 3, 4, 0, 4);
            resFile.Save("newU.bfres");*/
        }

        static void ConvertPlatform(string fileName)
        {
            ResFile resFile = new ResFile(fileName);
            resFile.Save($"{fileName}.new.bfres");

            resFile.ChangePlatform(true, 4096, 0, 5, 0, 3);
            resFile.Save($"{fileName}.newNX.bfres");
        }
    }
}
