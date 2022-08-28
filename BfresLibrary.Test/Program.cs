using System;
using System.IO;
using BfresLibrary;
using BfresLibrary.TextConvert;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ResFile animFile = new ResFile("Npc_BankaraIdolA.bfres.zs.dec");


          /*  animFile.Save("JugemObjRB.bfres");
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
            }*/
        }
    }
}
