using BfresLibrary;
using BfresLibrary.Helpers;
using BfresLibrary.Switch;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var resFile = new ResFile("PlayerMarioSuper.bfres");
            LoadMeshData(resFile);
            LoadTextureData(resFile);

            resFile.Save("PlayerMarioSuperRB.bfres");
        }

        static void LoadMeshData(ResFile resFile)
        {
            foreach (var model in resFile.Models.Values)
            {
                foreach (var shape in model.Shapes.Values)
                {
                    var material = model.Materials[shape.MaterialIndex];
                    var vertexBuffer = model.VertexBuffers[shape.VertexBufferIndex];

                    //Indices
                    var mesh = shape.Meshes[0]; //first LOD
                    var faces = mesh.GetIndices();

                    //Vertex data
                    VertexBufferHelper helper = new VertexBufferHelper(vertexBuffer, resFile.ByteOrder);
                    foreach (var att in helper.Attributes)
                    {
                        if (att.Name == "_p0") //positions
                        {
                            var vertex_positions = att.Data;
                        }
                        if (att.Name == "_n0") //normals
                        {
                            var vertex_normals = att.Data;
                        }
                        if (att.Name == "_u0") //uvs
                        {
                            var vertex_uvs = att.Data;
                        }
                    }
                }
            }
        }

        static void LoadTextureData(ResFile resFile)
        {
            foreach (var texture in resFile.Textures.Values)
            {
                var bntxTexture = ((SwitchTexture)texture).Texture; //Get raw bntx texture
                //Image must be decompressed by format using another library (astc, bcn, etc)
                var format = bntxTexture.Format;
                var imageData = texture.GetDeswizzledData(0, 0);
            }
        }
    }
}
