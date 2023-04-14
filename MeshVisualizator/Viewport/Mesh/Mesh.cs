using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Controls;

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
      public float Value;
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
                     ShaderType[] shader_types,
                     in Camera2D? camera)
      {
         this.shader_files = shader_files;
         this.shader_types = shader_types;
         ResetShader(camera, width, height);

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
      public void ResetShader(Camera2D? camera, float width, float height)
      {
         Projection = Camera2D.GetOrthoMatrix(PixelsToMeters(width), PixelsToMeters(height));
         Transform = camera!.GetTransformMatrix();
         
         shader?.Dispose();
         shader = new ShaderProgram(shader_files, shader_types);
         shader.LinkShaders();
         shader.setMatrix4("projection", Projection);
         shader.setMatrix4("transform", Transform);

         gridShader?.Dispose();
         gridShader = new ShaderProgram(new []{ @"Shaders/Grid shaders/fragment.glsl", @"Shaders/Grid shaders/vertex.glsl" }, 
                                        new[] { ShaderType.FragmentShader, ShaderType.VertexShader });
         gridShader.LinkShaders();
         gridShader.setMatrix4("projection", Projection);
         gridShader.setMatrix4("transform", Transform);
      }
      virtual public void Draw(Camera2D? camera)

      {
         Transform = camera!.GetTransformMatrix();

         if (isGridDrawing)
         {
            // draw grid
            gridShader.UseShaders();
            gridShader.setMatrix4("projection", Projection);
            gridShader.setMatrix4("transform", Transform);

            GL.LineWidth(2f);
            GL.BindVertexArray(VAOgrid);
            GL.DrawElements(BeginMode.Lines, indecesGrid.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
         }
         // draw vectors/scalars
         shader.UseShaders();
         shader.setMatrix4("projection", Projection);
         shader.setMatrix4("transform", Transform);

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
                                sizeof(float) * (this is not VectorMesh2D ? 6 : 3), // disgusting
                                0);
         GL.EnableVertexAttribArray(0);

         if (this is not VectorMesh2D) // i am stupid
         {
            GL.VertexAttribPointer(1,
                                   3,
                                   VertexAttribPointerType.Float,
                                   false,
                                   6 * sizeof(float),
                                   3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
         }
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
      virtual protected void GetMeshData(string elemsfile, string knotsfile, in ValueColorGradient vcg)
      {
         SetVertices(knotsfile, vcg);
         SetIndeces(elemsfile);
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
                  foreach (var num in elem.KnotNums)
                     indecesList.Add(num);
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
      abstract protected void SetVertices(string knotsfile, in ValueColorGradient vcg);
      abstract protected void SetBuffers();
   }
}
