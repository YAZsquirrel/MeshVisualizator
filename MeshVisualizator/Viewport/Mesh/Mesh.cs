using System;
using System.Collections.Generic;
using System.Linq;

namespace MeshVisualizator
{
   public enum MeshType { Quadrilateral, Triangle };
   enum FieldType
   {
      Scalar, 
      Vector,
      Both
   }
   struct Knot2D
   {
      public float X;
      public float Y;
   }
   struct Vector2D
   {
      public Vector2 Origin;
      public Vector2 Direction;
      public float Length => Direction.Length;
      public float Angle => MathF.Atan2(Direction.Y, Direction.X);
   }
   struct Element
   {
      public static int size;
      public List<uint> KnotNums;
   }
   public abstract class Mesh2D
   {

      readonly public MeshType meshType;
      private const float inchesInCm = 2.54f;
      public static float PixelsToMeters(float pixels) => pixels * inchesInCm / 9600f;
      public static float MetersToPixels(float meters) => meters / PixelsToMeters(1);

      protected float[] vertices;
      protected uint[] indeces;
      protected uint[] indecesGrid;

      protected ShaderProgram shader, gridShader;
      protected Matrix4 Projection;
      protected Matrix4 Transform;
      protected string[] shader_files;
      protected ShaderType[] shader_types;
      private int VAOgrid;
      private int VBOgrid;
      private int EBOgrid;
      public static bool isGridDrawing {set; get;}

      public Mesh2D(float width,
                     float height,
                     MeshType meshType,
                     string[] shader_files,
                     ShaderType[] shader_types)
      {
         this.shader_files = shader_files;
         this.shader_types = shader_types;
         ResetShader(width, height);

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


      }
      public void ResetShader(float width, float height)
      {
         Projection = Camera2D.Instance.GetOrthoMatrix(PixelsToMeters(width), PixelsToMeters(height));
         Transform = Camera2D.Instance.GetTransformMatrix();
         
         shader?.Dispose();
         shader = new ShaderProgram(shader_files, shader_types);
         shader.LinkShaders();
         shader.SetMatrix4("projection", Projection);
         shader.SetMatrix4("transform", Transform);
 
         gridShader?.Dispose();
         gridShader = new ShaderProgram(new []{ @"Shaders/Grid shaders/fragment.glsl", @"Shaders/Grid shaders/vertex.glsl" }, 
                                        new[] { ShaderType.FragmentShader, ShaderType.VertexShader });
         gridShader.LinkShaders();
         //gridShader.SetMatrix4("projection", Projection);
         //gridShader.SetMatrix4("transform", Transform);
      }
      virtual public void Draw()
      {
         if (isGridDrawing)
         {
            // draw grid
            gridShader.UseShaders();
            gridShader.SetMatrix4("projection", Projection);
            gridShader.SetMatrix4("transform", Transform);

            GL.LineWidth(1f);
            GL.BindVertexArray(VAOgrid);
            GL.DrawElements(BeginMode.Lines, indecesGrid.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
         }

      }
      private void SetGridBuffers()
      {
         VAOgrid = GL.GenVertexArray();
         VBOgrid = GL.GenBuffer();

         GL.BindVertexArray(VAOgrid);
         GL.BindBuffer(BufferTarget.ArrayBuffer, VBOgrid);
         GL.BufferData(BufferTarget.ArrayBuffer,
                       vertices.Length * sizeof(float),
                       vertices,
                       BufferUsageHint.StaticDraw);
         GL.VertexAttribPointer(0,
                                3,
                                VertexAttribPointerType.Float,
                                false,
                                sizeof(float) * 3, 
                                0);
         GL.EnableVertexAttribArray(0);

         EBOgrid = GL.GenBuffer();
         GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBOgrid);
         GL.BufferData(BufferTarget.ElementArrayBuffer,
                       indecesGrid.Length * sizeof(uint),
                       indecesGrid,
                       BufferUsageHint.StaticDraw);
         GL.BindVertexArray(0);
         gridShader.UseShaders();
      }
      virtual public void RemoveMesh()
      {
         GL.DeleteBuffer(VBOgrid);
         GL.DeleteBuffer(EBOgrid);
         GL.DeleteVertexArray(VAOgrid);
      }
      virtual protected void GetMeshData(string elemsfile, string knotsfile, string resultfile)
      {
         SetVertices(knotsfile);
         SetIndeces(elemsfile);
         SetResults(resultfile);
         SetGridBuffers();
      }
      virtual protected void SetIndeces(string elemsfile)
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
            Element elem = new Element { KnotNums = new List<uint>() };
            for (int j = 0; j < Element.size; j++)
               elem.KnotNums.Add(uint.Parse(number[i + j]));

            elems.Add(elem);
         }

         List<uint> indecesList = new List<uint>();
         List<uint> gridIndecesList = new List<uint>();
         switch (meshType)
         {
            case MeshType.Triangle:
               foreach (var elem in elems)
               {
                  uint[] elemNums = new uint[] {elem.KnotNums[0],
                                                elem.KnotNums[1],

                                                elem.KnotNums[1],
                                                elem.KnotNums[2],

                                                elem.KnotNums[0],
                                                elem.KnotNums[2]};
                  foreach (var num in elem.KnotNums)                          
                     indecesList.Add(num);
                  foreach (var num in elemNums)
                     gridIndecesList.Add(num);
               }
               break;

            case MeshType.Quadrilateral:
               foreach (var elem in elems)
               {
                  uint[] triagNums = new uint[]{elem.KnotNums[0],
                                                elem.KnotNums[1],
                                                elem.KnotNums[2],
                                                elem.KnotNums[1],
                                                elem.KnotNums[2],
                                                elem.KnotNums[3]};

                  uint[] elemNums = new uint[] {elem.KnotNums[0], 
                                                elem.KnotNums[1],
                                    
                                                elem.KnotNums[1],
                                                elem.KnotNums[3],
                                                
                                                elem.KnotNums[2],
                                                elem.KnotNums[3],
                                    
                                                elem.KnotNums[2],
                                                elem.KnotNums[0]};
                  foreach (var num in triagNums)
                     indecesList.Add(num);
                  foreach (var num in elemNums)
                     gridIndecesList.Add(num);
               }
               break;
         }

         indeces = indecesList.ToArray();
         indecesGrid = gridIndecesList.ToArray();
      }
      virtual protected void SetVertices(string knotsfile)
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

         vertices = new float[3 * knots.Count];

         for (int i = 0; i < knots.Count; i++)
         {
            vertices[i * 3 + 0] = knots[i].X;
            vertices[i * 3 + 1] = knots[i].Y;
            vertices[i * 3 + 2] = 0f;
         }

      }
      abstract protected void SetResults(string resultfile);
      abstract protected void SetBuffers();
   }
}
