using System.Collections.Generic;
using System.Linq;

namespace MeshVisualizator
{
   public sealed class ScalarMesh2D : Mesh2D
   {
      private int VAO = 0;
      private int VBO = 0;
      private int EBO = 0;

      public ScalarMesh2D(string elemsfile, string knotsfile, float width, float height, in ValueColorGradient vcg, in Camera2D? camera, MeshType meshType)
         : base(width, height, meshType , new[] { @"Shaders/Scalar mesh shaders/fragment.glsl", @"Shaders/Scalar mesh shaders/vertex.glsl" }, new[] { ShaderType.FragmentShader, ShaderType.VertexShader }, camera)
      {

         GetMeshData(elemsfile, knotsfile, vcg);
         SetBuffers();
      }
      protected override void SetVertices(string knotsfile, in ValueColorGradient vcg)
      {
         string[] number;
         using (StreamReader sr = new StreamReader(knotsfile))
         {
            string s = sr.ReadToEnd().Replace('.', ',');
            number = s.Split(new char[] { '\n', ' ', '\t', '\r' });
         }

         number = number.Where(w => w.Length > 0).ToArray();
         List<Knot2D> knots = new List<Knot2D>();
         for (int i = 0; i < number.Length; i += 3)
         {
            knots.Add(new Knot2D
            {
               X = float.Parse(number[i]),
               Y = float.Parse(number[i + 1]),
               Value = float.Parse(number[i + 2])
            });
         }

         var orderedKnots = knots.OrderBy(x => x.Value);
         vcg.MaxValue = orderedKnots.Last().Value;
         vcg.MinValue = orderedKnots.First().Value;

         List<float> vert_floats = new List<float>();
         foreach (var knot in knots)
         {
            vert_floats.Add(knot.X);
            vert_floats.Add(knot.Y);
            vert_floats.Add(0);
            
            Vector3 color = vcg.GetColorByValue(knot.Value);
            vert_floats.Add(color.X);
            vert_floats.Add(color.Y);
            vert_floats.Add(color.Z);
         }
         vertices = vert_floats.ToArray();
      }
      public override void Draw(Camera2D? camera)
      {

         GL.BindVertexArray(VAO);
         GL.DrawElements(PrimitiveType.Triangles, indeces.Length, DrawElementsType.UnsignedInt, 0);
         GL.BindVertexArray(0);

         base.Draw(camera);
      }
      protected override void SetBuffers()
      {
         VAO = GL.GenVertexArray();
         VBO = GL.GenBuffer();
         GL.BindVertexArray(VAO);
         GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
         GL.BufferData(BufferTarget.ArrayBuffer,
                       vertices.Length * sizeof(float),
                       vertices,
                       BufferUsageHint.StaticDraw);
         GL.VertexAttribPointer(0,
                                3,
                                VertexAttribPointerType.Float,
                                false,
                                6 * sizeof(float),
                                0);
         GL.EnableVertexAttribArray(0);
         GL.VertexAttribPointer(1,
                                3,
                                VertexAttribPointerType.Float,
                                false,
                                6 * sizeof(float),
                                3 * sizeof(float));
         GL.EnableVertexAttribArray(1);

         EBO = GL.GenBuffer();
         GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
         GL.BufferData(BufferTarget.ElementArrayBuffer,
                       indeces.Length * sizeof(uint),
                       indeces,
                       BufferUsageHint.StaticDraw);
         GL.BindVertexArray(0);
         shader.UseShaders();
      }
      public override void RemoveMesh()
      {
         GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
         GL.BindVertexArray(0);
         GL.UseProgram(0);
         GL.DeleteBuffer(VBO);
         GL.DeleteBuffer(EBO);
         GL.DeleteVertexArray(VAO);
      }
   }
}
