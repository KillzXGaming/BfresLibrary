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
            resFile.Save("newU.bfres");
        }
    }
}
