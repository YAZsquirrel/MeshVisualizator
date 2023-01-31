using System;
using System.Windows;
using Microsoft.Win32;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using static MeshVisualizator.Mesh2D;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;

namespace MeshVisualizator
{
   public partial class MainWindow : Window
   {
      Mesh2D? mesh;
      ValueColorGradient vcg;
      Camera2D camera;
      MeshType meshType = MeshType.Quadrilateral;
      public MainWindow()
      {
         InitializeComponent();

         glControl.Start(new GLWpfControlSettings() 
         { 
            MinorVersion = 5, 
            MajorVersion = 4, 
            GraphicsProfile = ContextProfile.Core,
         });
      }
      #region Scale
      private void R_Scale_Loaded(object sender, RoutedEventArgs e)
      {
         //vcg = new ValueColorGradient(Vector3.Zero, Vector3.One);  // white -> red
         //vcg = new ValueColorGradient(new Vector3(0, 0, 1) , new Vector3(1, 0, 0)); // blue -> red
         vcg = new ValueColorGradient(new[]{ new Vector3(0, 0, 1),
                                             new Vector3(0, 1, 1),
                                             //new Vector3(0, 1, 0),
                                             new Vector3(1, 1, 0),
                                             new Vector3(1, 0, 0)});
         DrawScale();
      }
      private void DrawScale()
      {
         var gradCollection = new GradientStopCollection();
         foreach (var ColorKnots in vcg.ColorKnots)
         {
            Color color = Color.FromRgb((byte)MathF.Round(ColorKnots.color.X * 255),
                                        (byte)MathF.Round(ColorKnots.color.Y * 255),
                                        (byte)MathF.Round(ColorKnots.color.Z * 255));
            gradCollection.Add(new GradientStop(color, ColorKnots.w));
         }

         R_Scale.Fill = new LinearGradientBrush(gradCollection, 90);
      }

      #endregion
      #region glContol
      Color4 color = new Color4(0.9f,0.9f,0.9f,1f);
      TimeSpan dt;
      private void glControl_Loaded(object sender, RoutedEventArgs e)
      {
         GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
         GL.Enable(EnableCap.DebugOutput);
         GL.Enable(EnableCap.DebugOutputSynchronous);

         camera = new();
         GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

         camera.Position.X = (float)glControl.ActualWidth / 2f;
         camera.Position.Y = (float)glControl.ActualHeight / 2f;
         camera.Position = -Vector3.UnitZ;
      }
      private void OpenTkControl_OnRender(TimeSpan delta)
      {
         dt = delta;
         GL.ClearColor(color);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

         mesh?.DrawMesh(camera);
         Title = camera?.Position.ToString() + " " + camera?.Scale.ToString();
         GL.Finish();
      }
      private void glControl_SizeChanged(object sender, SizeChangedEventArgs e)
      {    
         mesh?.ResetShader(camera, (float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void glControl_Unloaded(object sender, RoutedEventArgs e)
      {
         // Unbind all the resources by binding the targets to 0/null.
         mesh?.RemoveMesh();
      }
      Point PrevMousePos;
      private void glControl_MouseMove(object sender, MouseEventArgs e)
      {
         switch (e.LeftButton)
         {
            case MouseButtonState.Pressed:
               Point Pos = e.GetPosition(sender as IInputElement);
               Vector delta = PrevMousePos - Pos;
               camera.Position.X -= camera.Scale * (float)delta.X * (float)dt.TotalSeconds / 24f;
               camera.Position.Y += camera.Scale * (float)delta.Y * (float)dt.TotalSeconds / 24f;

               PrevMousePos = Pos; 
               break;
            case MouseButtonState.Released:
               PrevMousePos = e.GetPosition(sender as IInputElement);
               break;
         }
      }
      bool MouseOnControl = false;
      private void glControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
      {
         float direction = 0;
         if (e.Delta > 0)
         {
            camera.Scale *= 1.1f;
            direction = 1f;
         }
         else 
         {
            camera.Scale /= 1.1f;
            direction = -1f;
         }
         if (MouseOnControl)
         {
            Point Pos = e.GetPosition(sender as IInputElement);
            Vector delta = new Point((float)glControl.ActualWidth / 2f, (float)glControl.ActualHeight / 2f) - Pos;
            delta /= Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
            camera.Position.X -= direction * (float)delta.X * (float)dt.TotalSeconds;
            camera.Position.Y += direction * (float)delta.Y * (float)dt.TotalSeconds;
         }
         mesh?.ResetShader(camera, (float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void glControl_GotMouseCapture(object sender, MouseEventArgs e)
      {
         MouseOnControl = true;
         Cursor = Cursors.Cross;
      }
      private void glControl_LostMouseCapture(object sender, MouseEventArgs e)
      {
         MouseOnControl = false;
         Cursor = Cursors.Arrow;
      }
      #endregion
      #region UIElements
      private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         meshType = (MeshType)((ComboBox)sender).SelectedIndex;
      }
      private void L_AddVertices_Drop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var file = files[0];
            L_AddVertices.Content = file;
         }
         DrawMesh();
      }
      private void L_AddElements_Drop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var file = files[0];
            L_AddElements.Content = file;
         }
         DrawMesh();
      }
      private void B_SetToOrigin_Click(object sender, RoutedEventArgs e)
      {
         camera.Position = -Vector3.UnitZ;//new Vector3((float)glControl.ActualWidth / 2f, (float)glControl.ActualHeight / 2f, -1);
         camera.Scale = 1;
         mesh?.ResetShader(camera, (float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void B_AddElements_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog open = new OpenFileDialog();
         open.Filter = "Text files(*.txt)|*.txt";
         open.ShowDialog();
         string filename = open.FileName;
         L_AddElements.Content = filename;
      }
      private void B_AddVertices_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog open = new OpenFileDialog();
         open.Filter = "Text files(*.txt)|*.txt";
         open.ShowDialog();
         string filename = open.FileName;
         L_AddVertices.Content = filename;
      }
      private void B_Recompile_Click(object sender, RoutedEventArgs e)
      {
         mesh?.ResetShader(camera, (float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void B_Redraw_Click(object sender, RoutedEventArgs e)
      {
         DrawMesh();
      }
      private void CB_ShowGrid_Checked(object sender, RoutedEventArgs e)
      {
         GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
      }
      private void CB_ShowGrid_Unchecked(object sender, RoutedEventArgs e)
      {
         GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
      }
      #endregion
      public void DrawMesh()
      {
         mesh?.RemoveMesh();
         if (File.Exists(L_AddElements.Content as string) && File.Exists(L_AddVertices.Content as string))
            mesh = new Mesh2D(L_AddElements.Content as string,
                              L_AddVertices.Content as string,
                              meshType,
                              (float)glControl.ActualWidth,
                              (float)glControl.ActualHeight, vcg, camera);
         else if (L_AddElements.Content as string != "File" || L_AddVertices.Content as string != "File")
            MessageBox.Show($"File \"{L_AddElements.Content}\" or \"{L_AddVertices.Content}\" does not exist!");

         Label1.Content = $"{vcg.MinValue}";
         Label2.Content = $"{vcg.MinValue + (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label3.Content = $"{vcg.MinValue + 2f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label4.Content = $"{vcg.MinValue + 3f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label5.Content = $"{vcg.MinValue + 4f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label6.Content = $"{vcg.MinValue + 5f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label7.Content = $"{vcg.MinValue + 6f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label8.Content = $"{vcg.MinValue + 7f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label9.Content = $"{vcg.MinValue + 8f * (vcg.MaxValue - vcg.MinValue) / 10f}";
         Label10.Content = $"{vcg.MaxValue}";
      }
   }
}
