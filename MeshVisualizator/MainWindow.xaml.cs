using System;
using System.Windows;
using Microsoft.Win32;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using static MeshVisualizator.Mesh2D;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace MeshVisualizator
{
   public partial class MainWindow : Window
   {
      Mesh2D? mesh;
      public ValueColorGradient vcg;
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
         vcg.Sort();
         DataContext = vcg;
         DrawScale();

      }
      public void DrawScale()
      {
         var gradCollection = new GradientStopCollection();

         foreach (var ColorKnot in vcg.ColorKnots)
         {
            Vector3 vcolor = ColorKnot.GetColorVector();
            Color color = Color.FromRgb((byte)MathF.Round(vcolor.X * 255),
                                        (byte)MathF.Round(vcolor.Y * 255),
                                        (byte)MathF.Round(vcolor.Z * 255));
            gradCollection.Add(new GradientStop(color, 1f - ColorKnot.Weight));
            
         }
         R_Scale.Fill = new LinearGradientBrush(gradCollection, 90);

         TBox_1.Text = $"{vcg.GetValueByWeight(0f)}";
         TBox_2.Text = $"{vcg.GetValueByWeight(1f / 9f)}";
         TBox_3.Text = $"{vcg.GetValueByWeight(2f / 9f)}";
         TBox_4.Text = $"{vcg.GetValueByWeight(3f / 9f)}";
         TBox_5.Text = $"{vcg.GetValueByWeight(4f / 9f)}";
         TBox_6.Text = $"{vcg.GetValueByWeight(5f / 9f)}";
         TBox_7.Text = $"{vcg.GetValueByWeight(6f / 9f)}";
         TBox_8.Text = $"{vcg.GetValueByWeight(7f / 9f)}";
         TBox_9.Text = $"{vcg.GetValueByWeight(8f / 9f)}";
         TBox_10.Text = $"{vcg.GetValueByWeight(1f)}";
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
      double secTicks = 0;
      string fps = ""; 
      private void OpenTkControl_OnRender(TimeSpan delta)
      {
         dt = delta;
         GL.ClearColor(color);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

         mesh?.DrawMesh(camera);
         Title = $"Cam pos: {-camera?.Position}, Scale:{camera?.Scale}, " +
                 $"Mouse: px:({PrevMousePos.X}, {glControl.ActualHeight - PrevMousePos.Y}) " +
                 $"m:({-camera?.Position.X + 1f / camera?.Scale * PixelsToMeters((float)(PrevMousePos.X - glControl.ActualWidth/2f))}, " +
                 $"{-camera?.Position.Y + 1f / camera?.Scale * PixelsToMeters((float)(glControl.ActualHeight/2f - PrevMousePos.Y))}) " +
                 $"relative:({PrevMousePos.X / glControl.ActualWidth}, {1f - PrevMousePos.Y / glControl.ActualHeight}) " + fps;

         if (secTicks > 0.5f)
         {
            secTicks = 0f;
            fps = $"FPS: {Math.Round(1f / delta.TotalSeconds)}";
         }
         secTicks += delta.TotalSeconds;

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
      const float cameraSpeed = 100f;
      private void glControl_MouseMove(object sender, MouseEventArgs e)
      {
         switch (e.LeftButton)
         {
            case MouseButtonState.Pressed:
               Point Pos = e.GetPosition(sender as IInputElement);
               Vector delta = PrevMousePos - Pos;
               camera.Position.X -=  PixelsToMeters((float)delta.X);
               camera.Position.Y +=  PixelsToMeters((float)delta.Y);

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
         const float scaleFactor = 2f;

         if (e.Delta < 0)
         {
            camera.Scale *= scaleFactor;
            //direction = cameraSpeed;
            direction = 1f;
         }
         else 
         {
            camera.Scale /= scaleFactor;
            //direction = -cameraSpeed;
            direction = -1f;
         }
         if (MouseOnControl)
         {
            Point Pos = e.GetPosition(sender as IInputElement);
            Vector delta = new Vector(Pos.X - (float)glControl.ActualWidth / 2f, (float)glControl.ActualHeight / 2f - Pos.Y);

            camera.Position.X -= direction * PixelsToMeters((float)delta.X) / scaleFactor;
            camera.Position.Y -= direction * PixelsToMeters((float)delta.Y) / scaleFactor;
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
         try
         {
            DrawSolution();
            DrawScale();
         }
         catch 
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show($"File {L_AddVertices.Content} does not satisfy the format or is corrupted!");
         }

      }
      private void L_AddElements_Drop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var file = files[0];
            L_AddElements.Content = file;
         }
         try
         {
            DrawSolution();
            DrawScale();
         }
         catch
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show($"File {L_AddElements.Content} does not satisfy the format or is corrupted!");
         }
      }
      private void B_SetToOrigin_Click(object sender, RoutedEventArgs e)
      {
         camera.Position = -Vector3.UnitZ;//new Vector3((float)glControl.ActualWidth / 2f, (float)glControl.ActualHeight / 2f, -1);
         camera.Position = new Vector3(-PixelsToMeters((float)glControl.ActualWidth / 2f), -PixelsToMeters((float)glControl.ActualHeight / 2f), -1);
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
         try
         {
            DrawSolution();
            DrawScale();
         }
         catch
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show($"File {filename} does not satisfy the format or is corrupted!");
         }
      }
      private void B_AddVertices_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog open = new OpenFileDialog();
         open.Filter = "Text files(*.txt)|*.txt";
         open.ShowDialog();
         string filename = open.FileName;
         L_AddVertices.Content = filename;
         try
         {
            DrawSolution();
            DrawScale();
         }
         catch
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show($"File {filename} does not satisfy the format or is corrupted!");
         }
      }
      private void B_Recompile_Click(object sender, RoutedEventArgs e)
      {
         mesh?.ResetShader(camera, (float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void B_Redraw_Click(object sender, RoutedEventArgs e)
      {
         DrawSolution();
         DrawScale();
      }
      private void CB_ShowGrid_Checked(object sender, RoutedEventArgs e)
      {
         GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
      }
      private void CB_ShowGrid_Unchecked(object sender, RoutedEventArgs e)
      {
         GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
      }
      private void CB_Logarithmic_Checked(object sender, RoutedEventArgs e)
      {
         vcg.SetIsLogarithmic = true;
         DrawScale();
      }
      private void CB_Logarithmic_Unchecked(object sender, RoutedEventArgs e)
      {
         vcg.SetIsLogarithmic = false;
         DrawScale();
      }
      private void B_AddNewColorKnot_Click(object sender, RoutedEventArgs e)
      {
         vcg.AddNewColorKnot();
         DrawScale();
         DrawSolution();
      }
      #endregion
      public void DrawSolution()
      {
         mesh?.RemoveMesh();
         mesh = null;
         if (File.Exists(L_AddElements.Content as string) && File.Exists(L_AddVertices.Content as string))
            mesh = new Mesh2D(L_AddElements.Content as string,
                              L_AddVertices.Content as string,
                              meshType,
                              (float)glControl.ActualWidth,
                              (float)glControl.ActualHeight, vcg, camera);
         else if (L_AddElements.Content as string != "File" && L_AddVertices.Content as string != "File")
            if (!File.Exists(L_AddElements.Content as string))
               MessageBox.Show($"File \"{L_AddElements.Content}\" does not exist!");
            else if (!File.Exists(L_AddVertices.Content as string))
               MessageBox.Show($"File \"{L_AddVertices.Content}\" does not exist!");

      }
   }
}
