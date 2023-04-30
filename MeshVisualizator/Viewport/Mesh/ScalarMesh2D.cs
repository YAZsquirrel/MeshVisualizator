using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace MeshVisualizator
{
   public sealed class ScalarMesh2D : Mesh2D
   {
      private const int scaleTextureResolution = 16;
      private int VAO = 0;
      private int VBO = 0;
      private int EBO = 0;
      private int textureID = 0;
      private float[] TexPixels;
      private float[] model;

      public ScalarMesh2D(string elemsfile, string knotsfile, string resultfile, float width, float height, MeshType meshType)
         : base(width, height, meshType, new[] { @"Shaders/Scalar mesh shaders/fragment.glsl", @"Shaders/Scalar mesh shaders/vertex.glsl" }, 
                                         new[] { ShaderType.FragmentShader, ShaderType.VertexShader })
      { 
         GetMeshData(elemsfile, knotsfile, resultfile);
         TexPixels = new float[3 * scaleTextureResolution]; // rgb
         ValueColorGradient.Instance.MakeScaleTexture(ref TexPixels);
         SetBuffers();
      }
      public override void Draw()
      {
         Transform = Camera2D.Instance.GetTransformMatrix();
         shader.UseShaders();
         shader.SetMatrix4("projection", Projection);
         shader.SetMatrix4("transform", Transform);

         GL.ActiveTexture(TextureUnit.Texture0);
         GL.BindTexture(TextureTarget.Texture1D, textureID);
         GL.BindVertexArray(VAO);
         GL.DrawElements(PrimitiveType.Triangles, indeces.Length, DrawElementsType.UnsignedInt, 0);
         GL.BindVertexArray(0);
         //GL.BindTexture(TextureTarget.Texture1D, 0);

         base.Draw();
      }
      protected override void SetBuffers()
      {

         VAO = GL.GenVertexArray();
         VBO = GL.GenBuffer();
         GL.BindVertexArray(VAO);
         GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

         GL.BufferData(BufferTarget.ArrayBuffer,
                       model.Length * sizeof(float),
                       model,
                       BufferUsageHint.StreamDraw);
         GL.VertexAttribPointer(0,
                                3,
                                VertexAttribPointerType.Float,
                                false,
                                4 * sizeof(float),
                                0);
         GL.EnableVertexAttribArray(0);
         GL.VertexAttribPointer(1,
                                1,
                                VertexAttribPointerType.Float,
                                false,
                                4 * sizeof(float),
                                3 * sizeof(float));
         GL.EnableVertexAttribArray(1);

         EBO = GL.GenBuffer();
         GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
         GL.BufferData(BufferTarget.ElementArrayBuffer,
                       indeces.Length * sizeof(uint),
                       indeces,
                       BufferUsageHint.StaticDraw);
         GL.BindVertexArray(0);


         textureID = GL.GenTexture();
         //GL.ActiveTexture(TextureUnit.Texture0); // unnessessary
         GL.BindTexture(TextureTarget.Texture1D, textureID);
         GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
         GL.TexImage1D(TextureTarget.Texture1D, 
                       0,
                       PixelInternalFormat.Rgb,
                       scaleTextureResolution, 
                       0, 
                       PixelFormat.Rgb, 
                       PixelType.Float, 
                       TexPixels);

         GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
         GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
         GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
         GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
         GL.BindTexture(TextureTarget.Texture1D, 0);

         shader.UseShaders();
      }
      public override void RemoveMesh()
      {
         GL.BindTexture(TextureTarget.Texture1D, 0);
         GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
         GL.BindVertexArray(0);
         GL.UseProgram(0);
         GL.DeleteBuffer(VBO);
         GL.DeleteBuffer(EBO);
         GL.DeleteVertexArray(VAO);

         base.RemoveMesh();
      }
      protected override void SetResults(string resultfile)
      {
         string[] number;
         using (StreamReader sr = new StreamReader(resultfile))
         {
            string s = sr.ReadToEnd().Replace('.', ',');
            number = s.Split(new char[] { '\n', ' ', '\t', '\r' });
         }

         number = number.Where(w => w.Length > 0).ToArray();
         List<float> scalars = new List<float>();
         for (int i = 0; i < number.Length; i++ )
            scalars.Add(float.Parse(number[i]));

         ValueColorGradient vcg = ValueColorGradient.Instance;
         vcg.MaxValue = scalars.Max();
         vcg.MinValue = scalars.Min();

         model = new float[vertices.Length + scalars.Count];

         for (int i = 0; i < model.Length; i+=4)
         {            
            model[i + 0] = vertices[i / 4 * 3 + 0];
            model[i + 1] = vertices[i / 4 * 3 + 1];
            model[i + 2] = vertices[i / 4 * 3 + 2];
            model[i + 3] = vcg.GetWeightByValue(scalars[i / 4]);
         }
         
      }
   }
}
