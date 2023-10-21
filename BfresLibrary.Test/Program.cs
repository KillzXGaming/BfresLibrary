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
            var resFileR = new ResFile("PlayerMarioSuper.bfres");
            resFileR.Save("PlayerMarioSuperRB.bfres");

            return;

            var resFile = new ResFile("CapManAnimationRB.bfres");

            foreach (var vis in resFile.BoneVisibilityAnims.Values)
            {
                var visOG = resFile.BoneVisibilityAnims[vis.Name];
                for (int i = 0; i < vis.BaseDataList?.Length; i++)
                {
                    if (vis.BaseDataList[i] != visOG.BaseDataList[i])
                        throw new Exception();
                }
            }

            // 


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
