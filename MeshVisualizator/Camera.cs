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
         Matrix4.CreateOrthographic(width, height, -1, 1);
      public Matrix4 GetTransformMatrix() =>
         //Matrix4.LookAt(Position, Position + new Vector3(0, 0, 1), new Vector3(0,1,0)) * Matrix4.CreateScale(Scale);
         Matrix4.CreateTranslation(Position) * Matrix4.CreateScale(Scale);

   }
}
