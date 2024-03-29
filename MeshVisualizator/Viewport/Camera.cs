﻿namespace MeshVisualizator
{
   public class Camera2D
   {
      public static Camera2D Instance { get; } = new Camera2D();

      public float Scale = 1;
      public Vector3 Position;
      private Camera2D()
      {
         Position = Vector3.Zero;
      }

      public Matrix4 GetOrthoMatrix(float width, float height) =>
         Matrix4.CreateOrthographic(width, height, -10, 10);
      public Matrix4 GetTransformMatrix() =>
         Matrix4.CreateScale(Scale) * 
         Matrix4.CreateTranslation(Position);

   }
}
