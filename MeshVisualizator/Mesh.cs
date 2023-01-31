using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MeshVisualizator
{
   struct Knot 
   {
      public float x;
      public float y;
      public float z;
      public float Value;
   }
   struct Element
   {
      public static int size;
      public List<int> KnotNums;
   }
   public class Mesh2D
   {
      public static float PixelsToMeters(float pixels) => pixels * 2.54f / 9600f;
      public static float MetersToPixels(float meters) => meters / PixelsToMeters(1);
      public enum MeshType { Quadrilateral, Triangle };

      readonly public MeshType meshType;

      private float[] vertices;
      private uint[] indeces;
      private int VAO = 0;
      private int VBO = 0;
      private int EBO = 0;
      private ShaderProgram shader;
      private Matrix4 Projection;
      private Matrix4 Transform;

      public Mesh2D (string elemsfile, 
                     string knotsfile, 
                     MeshType meshType,
                     float width,
                     float height, 
                     in ValueColorGradient vcg,
                     in Camera2D? camera)
      {
         this.meshType = meshType;
         switch (meshType)
         {
            case MeshType.Triangle:
               Element.size = 3;
               break;
            case MeshType.Quadrilateral:
               Element.size = 4;
               break;
         }
         GetMeshData(elemsfile, knotsfile, vcg);
         ResetShader(camera, width, height);
         SetBuffers();
      }

      public void ResetShader(Camera2D? camera, float width, float height)
      {
         shader?.Dispose();
         shader = new ShaderProgram(new[] { "fragshader.glsl", "vertexshader.glsl" },
                                    new[] { ShaderType.FragmentShader, ShaderType.VertexShader });
         shader.LinkShaders();
         Projection = Camera2D.GetOrthoMatrix(PixelsToMeters(width), PixelsToMeters(height));
         //Projection = Matrix4.CreatePerspectiveFieldOfView(camera.Scale * MathHelper.Pi / 4f, width / height, 0.01f, 100f);
         //Projection = Camera2D.GetOrthoMatrix(width, height);
         Transform = camera.GetTransformMatrix();
         shader.setMatrix4("projection", Projection);
         shader.setMatrix4("transform", Transform);

      }
      public void DrawMesh(Camera2D? camera)
      {
         shader.UseShaders();
         Transform = camera.GetTransformMatrix();
         shader.setMatrix4("projection", Projection);
         shader.setMatrix4("transform", Transform);

         GL.BindVertexArray(VAO);
         GL.DrawElements(BeginMode.Triangles, indeces.Length, DrawElementsType.UnsignedInt, 0);
         GL.BindVertexArray(0);
      }
      public void RemoveMesh()
      {
         GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
         GL.BindVertexArray(0);
         GL.UseProgram(0);
         GL.DeleteBuffer(VBO);
         GL.DeleteVertexArray(VAO);
      }
      private void GetMeshData(string elemsfile, string knotsfile, in ValueColorGradient vcg)
      {
         SetVertices(knotsfile, vcg);
         SetIndeces(elemsfile);
      }
      private void SetIndeces(string elemsfile)
      {
         string[] number;
         using (StreamReader sr = new StreamReader(@elemsfile))
         {
            string s = sr.ReadToEnd().Replace('.', ',');
            number = s.Split(new char[] { '\n', ' ', '\t', '\r' });
         }

         number = number.Where(w => w.Length > 0).ToArray();

         List<Element> elems = new List<Element>();

         for (int i = 0; i < number.Length; i += Element.size)
         {
            Element elem = new Element{ KnotNums = new List<int>() };
            for (int j = 0; j < Element.size; j++)
               elem.KnotNums.Add(int.Parse(number[i + j]));

            elems.Add(elem);
         }

         List<uint> indecesList = new List<uint>();
         switch (meshType)
         {
            case MeshType.Triangle:
               foreach (var elem in elems)
                  foreach (var num in elem.KnotNums)
                     indecesList.Add((uint)num);
               break;

            case MeshType.Quadrilateral:
               foreach (var elem in elems)
               {
                  uint[] triagNums = new uint[]{(uint)elem.KnotNums[0],
                                                (uint)elem.KnotNums[1],
                                                (uint)elem.KnotNums[2],
                                                (uint)elem.KnotNums[1],
                                                (uint)elem.KnotNums[2],
                                                (uint)elem.KnotNums[3]};
                  foreach (var num in triagNums)
                     indecesList.Add(num);
               }
               break;
         }

         indeces = indecesList.ToArray();

      }
      private void SetVertices(string knotsfile, in ValueColorGradient vcg)
      {
         string[] number;
         using (StreamReader sr = new StreamReader(@knotsfile))
         {
            string s = sr.ReadToEnd().Replace('.', ',');
            number = s.Split(new char[] { '\n', ' ', '\t', '\r' });
         }

         number = number.Where(w => w.Length > 0).ToArray();
         List<Knot> knots = new List<Knot>();
         for (int i = 0; i < number.Length; i += 3)
         {
            knots.Add(new Knot
            {
               x = float.Parse(number[i]),
               y = float.Parse(number[i + 1]),
               z = 0f,
               Value = float.Parse(number[i + 2])
            });
         }

         var orderedKnots = knots.OrderBy(x => x.Value);
         vcg.MaxValue = orderedKnots.Last().Value;
         vcg.MinValue = orderedKnots.First().Value;

         List<float> vert_floats= new List<float>();  
         foreach (var knot in knots)
         {
            vert_floats.Add(knot.x);
            vert_floats.Add(knot.y);
            vert_floats.Add(knot.z);
            Vector3 color = vcg.GetColorByValue(knot.Value);
            vert_floats.Add(color.X);
            vert_floats.Add(color.Y);
            vert_floats.Add(color.Z);
         }
         vertices = vert_floats.ToArray();
      }
      private void SetBuffers()
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
   }
}
