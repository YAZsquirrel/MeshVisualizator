namespace MeshVisualizator
{
   public class Camera2D
   {
      public float Scale = 1;
      public Vector3 Position;
      public Camera2D()
      {
         Position = Vector3.Zero;
      }
      public static Matrix4 GetOrthoMatrix(float width, float height) =>
         Matrix4.CreateOrthographic(width, height, -10000, 10000);
      public Matrix4 GetTransformMatrix() =>
         Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);

   }
}
