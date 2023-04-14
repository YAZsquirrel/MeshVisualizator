using System;
using System.Collections.Generic;
using System.Linq;

namespace MeshVisualizator
{
   public sealed class VectorMesh2D : Mesh2D
   {
      struct Arrow
      {
         public int VAO, VBO, EBO;
         public float[] vertices;
         public int[] indeces;
         public ArrowType arrowType;
         public void ResetBuffers()
         {
            int verts = 0;
            switch (arrowType)
            {
               case ArrowType.Lines:
               case ArrowType.ThickLines:
                  verts = 2;
                  break;
               case ArrowType.Thin:
               case ArrowType.Thick:
                  verts = 2; break;
            }

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          vertices.Length * sizeof(float),
                          vertices,
                          BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0,
                                   verts,
                                   VertexAttribPointerType.Float,
                                   false,
                                   verts * sizeof(float),
                                   0);
            GL.EnableVertexAttribArray(0);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          indeces.Length * sizeof(uint),
                          indeces,
                          BufferUsageHint.StaticDraw);
            GL.BindVertexArray(0);
         }
      } 

      public static float VectorLength { get; set; }
      public static bool IsLengthConsistent { get; internal set; }
      private float lineWidth = 2f;

      Arrow arrow;
      public enum ArrowType {None = -1, Lines = 0, ThickLines, Thin, Thick }
      private int VBOmat, VBOcol;

      Matrix4[]? arrow_mats;
      private Vector3[]? colors;

      public VectorMesh2D(string elemsfile, string knotsfile, string vectorfile, float width, float height, in ValueColorGradient vcg, in Camera2D? camera, ArrowType arrowType, MeshType meshType)
         : base(width, height, meshType, new[] { @"Shaders/Vector mesh shaders/fragment.glsl", @"Shaders/Vector mesh shaders/vertex.glsl" }, 
                                         new[] { ShaderType.FragmentShader, ShaderType.VertexShader }, camera)
      {

         arrow.arrowType = arrowType;
         GetMeshData(elemsfile, knotsfile, vcg);
         SetVectors(vectorfile, vcg);
         ChangeArrowType(arrowType);
         SetBuffers();
      }

      private void SetVectors(string vectorfile, ValueColorGradient vcg)
      {
         string[] number;
         using (StreamReader sr = new StreamReader(vectorfile))
         {
            string s = sr.ReadToEnd().Replace('.', ',');
            number = s.Split(new char[] { '\n', ' ', '\t', '\r' });
         }

         number = number.Where(w => w.Length > 0).ToArray();
         List<Vector2D> vectors = new List<Vector2D>();
         for (int i = 0; i < number.Length; i += 4)
         {
            vectors.Add(new Vector2D
            {
               Origin = new Vector2 { X = float.Parse(number[i]), Y = float.Parse(number[i + 1]) },
               Direction = new Vector2 { X = float.Parse(number[i + 2]), Y = float.Parse(number[i + 3]) }
            });
         }

         var orderedVectors = vectors.OrderBy(x => x.Length);
         vcg.MaxValue = orderedVectors.Last().Length;
         vcg.MinValue = orderedVectors.First().Length;


         arrow_mats = new Matrix4[vectors.Count];

         colors = new Vector3[vectors.Count];

         for (int i = 0; i < vectors.Count; i++)
         {
            float len = IsLengthConsistent ? (vcg.MaxValue + vcg.MinValue) / 2f : vectors[i].Length;
            arrow_mats[i] = Matrix4.CreateScale( len * VectorLength, len * VectorLength, 0)
                          * Matrix4.CreateRotationZ(vectors[i].Angle)
                          * Matrix4.CreateTranslation(new Vector3(vectors[i].Origin));

            colors[i] = vcg.GetColorByValue(vectors[i].Length);
         }

      }

      PrimitiveType pt = PrimitiveType.Lines;
      public override void Draw(Camera2D? camera)
      {
         base.Draw(camera);

         GL.LineWidth(lineWidth);
         GL.BindVertexArray(arrow.VAO);
         GL.DrawElementsInstanced(pt, arrow.indeces.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, arrow_mats.Length);
         GL.BindVertexArray(0);
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
         for (int i = 0; i < number.Length; i += 2)
         {
            knots.Add(new Knot2D
            {
               X = float.Parse(number[i]),
               Y = float.Parse(number[i + 1])
            });
         }

         List<float> vert_floats = new List<float>();
         foreach (var knot in knots)
         {
            vert_floats.Add(knot.X);
            vert_floats.Add(knot.Y);
            vert_floats.Add(0);
         }
         vertices = vert_floats.ToArray();
      }
      public override void RemoveMesh()
      {
         GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
         GL.BindVertexArray(0);
         GL.UseProgram(0);
         GL.DeleteBuffer(VBOmat);
         GL.DeleteBuffer(VBOcol);
         GL.DeleteVertexArray(arrow.VAO);

         arrow_mats = null;
         arrow.indeces = null;
         arrow.vertices = null;

         base.RemoveMesh();
      }
      public void ChangeArrowType(ArrowType arrowType)
      {
         arrow.arrowType = arrowType;
         switch (arrow.arrowType)
         {
            case ArrowType.Lines:
            case ArrowType.ThickLines:
               arrow.vertices = new float[]{ 0.0f, 0.0f,
                                             1.0f, 0.0f,
                                              0.625f,-0.25f,
                                              0.625f, 0.25f};

               arrow.indeces = new int[] { 0, 1,
                                            1, 2,
                                            1, 3 };
               pt = PrimitiveType.Lines;
               break;

            case ArrowType.Thin:
               arrow.vertices = new float[]{ 1.0f, 0.0f,
                                             0.625f, -0.1875f,
                                             0.625f, 0.1875f, 
                                             0f, -1/16f,
                                             0f, 1/16f,
                                             0.625f, -1/16f,
                                             0.625f,  1/16f  };
               arrow.indeces = new int[] { 0, 1, 2, 
                                           3, 4, 5,
                                           4, 6, 5};
               pt = PrimitiveType.Triangles; break;

            case ArrowType.Thick:
               arrow.vertices = new float[]{ 1.0f, 0.0f,
                                             0.625f, -0.25f,
                                             0.625f, 0.25f,
                                             0f, -0.125f,
                                             0f, 0.125f,
                                             0.625f, -0.125f,
                                             0.625f,  0.125f  };
               arrow.indeces = new int[] { 0, 1, 2,
                                            3, 4, 5,
                                            4, 6, 5};
               pt = PrimitiveType.Triangles; break;
         }


         lineWidth = 5.0f;
         if (arrow.arrowType == ArrowType.ThickLines)
            lineWidth = 10f;

         arrow.ResetBuffers();
         SetBuffers();
      }
      protected override void SetBuffers()
      {
         VBOcol = GL.GenBuffer();
         GL.BindBuffer(BufferTarget.ArrayBuffer, VBOcol);
         GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * 3 * sizeof(float), colors, BufferUsageHint.StaticDraw);

         GL.BindVertexArray(arrow.VAO);
         GL.EnableVertexAttribArray(1);
         GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, 3 * sizeof(float), 0);

         GL.VertexAttribDivisor(1, 1);

         VBOmat = GL.GenBuffer();
         GL.BindBuffer(BufferTarget.ArrayBuffer, VBOmat);
         GL.BufferData(BufferTarget.ArrayBuffer, arrow_mats.Length * 16 * sizeof(float), arrow_mats, BufferUsageHint.StaticDraw);
         // vertex attributes
         int vec4Size = 4 * sizeof(float);

         GL.EnableVertexAttribArray(3);
         GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 4 * vec4Size, 0);
         GL.EnableVertexAttribArray(4);
         GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 4 * vec4Size, (1 * vec4Size));
         GL.EnableVertexAttribArray(5);
         GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 4 * vec4Size, (2 * vec4Size));
         GL.EnableVertexAttribArray(6);
         GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 4 * vec4Size, (3 * vec4Size));

         //GL.VertexAttribDivisor(2, 1);
         GL.VertexAttribDivisor(3, 1);
         GL.VertexAttribDivisor(4, 1);
         GL.VertexAttribDivisor(5, 1);
         GL.VertexAttribDivisor(6, 1);

         GL.BindVertexArray(0);
         shader.UseShaders();
      }
   }
}
