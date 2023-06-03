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
            new ResFile("ExternalBinaryString.bfres");
            ResFile animFile = new ResFile("Animal_Bass.Bass.bfres");


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
