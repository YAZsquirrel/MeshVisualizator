using System;
using System.Windows;
using Microsoft.Win32;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using static MeshVisualizator.Mesh2D;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

namespace MeshVisualizator
{
   public partial class MainWindow : Window
   {
      Mesh2D? mesh;
      public ValueColorGradient vcg = ValueColorGradient.Instance;
      Camera2D camera = Camera2D.Instance;
      MeshType meshType = MeshType.Quadrilateral;
      FieldType fieldType = FieldType.Scalar;

      VectorMesh2D.ArrowType ArrowType = VectorMesh2D.ArrowType.Lines;

      public MainWindow()
      {
         InitializeComponent();

         glControl.Start(new GLWpfControlSettings() 
         { 
            MinorVersion = 5, 
            MajorVersion = 4, 
            GraphicsProfile = ContextProfile.Core,
         });
         GL.Enable(EnableCap.LineSmooth);
         GL.GetFloat(GetPName.AliasedLineWidthRange, new float[] { 2, 10 });
      }
      #region Scale
      private void R_Scale_Loaded(object sender, RoutedEventArgs e)
      {
         //vcg = new ValueColorGradient(Vector3.Zero, Vector3.One);  // white -> red
         //vcg = new ValueColorGradient(new Vector3(0, 0, 1) , new Vector3(1, 0, 0)); // blue -> red

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
      #region Glcontrol
      Color4 color = new(0.9f,0.9f,0.9f,1f);
      private void glControl_Loaded(object sender, RoutedEventArgs e)
      {
         GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
         GL.Enable(EnableCap.DebugOutput);
         GL.Enable(EnableCap.DebugOutputSynchronous);
         GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

         camera.Position.X = (float)glControl.ActualWidth / 2f;
         camera.Position.Y = (float)glControl.ActualHeight / 2f;
         camera.Position = -Vector3.UnitZ;
      }
      double secTicks = 0;
      string fps = ""; 
      private void OpenTkControl_OnRender(TimeSpan delta)
      {
         GL.ClearColor(color);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
         mesh?.Draw();

         var sb = new StringBuilder();
         Title = sb.Append($"Cam pos: {-camera.Position}, Scale:{camera.Scale}, " +
                           $"Mouse: px:({PrevMousePos.X}, {glControl.ActualHeight - PrevMousePos.Y}) " +
                           $"m:({-camera.Position.X + 1f / camera.Scale * PixelsToMeters((float)(PrevMousePos.X - glControl.ActualWidth / 2f))}, " +
                           $"{-camera.Position.Y + 1f / camera.Scale * PixelsToMeters((float)(glControl.ActualHeight / 2f - PrevMousePos.Y))}) " +
                           $"relative:({PrevMousePos.X / glControl.ActualWidth}, {1f - PrevMousePos.Y / glControl.ActualHeight}) " + fps).ToString();

         if (secTicks > 1f)
         {
            secTicks = 0f;
            fps = $"FPS: {Math.Round(1f / delta.TotalSeconds)}";
         }
         secTicks += delta.TotalSeconds;

         GL.Finish();
      }
      private void glControl_SizeChanged(object sender, SizeChangedEventArgs e)
      {    
         mesh?.ResetShader((float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void glControl_Unloaded(object sender, RoutedEventArgs e)
      {
         // Unbind all the resources by binding the targets to null.
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
         float direction;
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
         mesh?.ResetShader((float)glControl.ActualWidth, (float)glControl.ActualHeight);
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
      private void CBox_MeshType_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         meshType = (MeshType)((ComboBox)sender).SelectedIndex;
         try
         {
            SetMesh();
         }
         catch
         {
            MessageBox.Show($"This mesh is not {meshType}");
         }
      }
      private void CBox_fieldType_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         fieldType = (FieldType)((ComboBox)sender).SelectedIndex;
         if (G_VectorParams is not null)
            G_VectorParams.IsEnabled = fieldType == FieldType.Vector;
         L_AddResults.Content = "Result file";
         mesh?.RemoveMesh();
         mesh = null;
      }
      private void CBox_Vectors_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (mesh is VectorMesh2D)
            (mesh as VectorMesh2D)?.ChangeArrowType(ArrowType = (VectorMesh2D.ArrowType)((ComboBox)sender).SelectedIndex);
         SetMesh();
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
            SetMesh();
            DrawScale();
         }
         catch (Exception ex)
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace?.ToString());
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
            SetMesh();
            DrawScale();
         }
         catch (Exception ex)
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace?.ToString());
         }
      }
      private void L_AddResults_Drop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var file = files[0];
            L_AddResults.Content = file;
         }
         try
         {
            SetMesh();
            DrawScale();
         }
         catch
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show($"Results were not of the {fieldType} field or file was corrupted!");
         }
      }
      private void B_SetToOrigin_Click(object sender, RoutedEventArgs e)   
      {
         camera.Position = -Vector3.UnitZ;//new Vector3((float)glControl.ActualWidth / 2f, (float)glControl.ActualHeight / 2f, -1);
         //camera.Position = new Vector3(-PixelsToMeters((float)glControl.ActualWidth / 2f), -PixelsToMeters((float)glControl.ActualHeight / 2f), -1);
         camera.Scale = 1;
         mesh?.ResetShader((float)glControl.ActualWidth, (float)glControl.ActualHeight);
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
            SetMesh();
            DrawScale();
         }
         catch (Exception ex)
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace?.ToString());
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
            SetMesh();
            DrawScale();
         }
         catch (Exception ex)
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace?.ToString());
         }
      }
      private void B_AddResults_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog open = new OpenFileDialog();
         open.Filter = "Text files(*.txt)|*.txt";
         open.ShowDialog();
         string filename = open.FileName;
         L_AddResults.Content = filename;
         try
         {
            SetMesh();
            DrawScale();
         }
         catch (Exception ex)
         {
            mesh?.RemoveMesh();
            mesh = null;
            MessageBox.Show(ex.Message + "\n" + ex.StackTrace?.ToString());
         }
      }
      private void B_Recompile_Click(object sender, RoutedEventArgs e)
      {
         mesh?.ResetShader((float)glControl.ActualWidth, (float)glControl.ActualHeight);
      }
      private void B_Redraw_Click(object sender, RoutedEventArgs e)
      {
         SetMesh();
         DrawScale();
      }
      private void CB_ShowGrid_Checked(object sender, RoutedEventArgs e)
      {
         //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
         Mesh2D.isGridDrawing = true;
      }
      private void CB_ShowGrid_Unchecked(object sender, RoutedEventArgs e)
      {
         //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
         Mesh2D.isGridDrawing = false;
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
         SetMesh();
      }
      private void MI_OpenPalette_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog open = new OpenFileDialog();
         open.Filter = "(Scale palette files *.spf)|*.spf";
         open.ShowDialog();

         vcg.SetPalette(open.FileName);
         DrawScale();
         SetMesh();
      }
      private void MI_SavePalette_Click(object sender, RoutedEventArgs e)
      {
         SaveFileDialog save = new SaveFileDialog();
         save.Filter = "(Scale palette files *.spf)|*.spf";
         save.ShowDialog();

         vcg.SavePalette(save.FileName);
      }
      private void MI_About_Click(object sender, RoutedEventArgs e)
      {
         string about = 
         """
            Author: Artyom Karalchuk (YAZsquirrel)
            Github link: https://github.com/YAZsquirrel/MeshVisualizator

            Press OK to copy to your clipboard
         """;


         switch (MessageBox.Show(about, "About", MessageBoxButton.OKCancel))
         {
            case MessageBoxResult.OK: 
               Clipboard.SetText(@"github.com/YAZsquirrel/MeshVisualizator"); return;
            case MessageBoxResult.Cancel: return;
         }

      }
      private void CBox_ConsiderLen_Checked(object sender, RoutedEventArgs e)
      {
         VectorMesh2D.IsLengthConsistent = false;
         SetMesh();
      }
      private void CBox_ConsiderLen_Unchecked(object sender, RoutedEventArgs e)
      {
         VectorMesh2D.IsLengthConsistent = true;
         SetMesh();

      }
      private void S_VecLen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         VectorMesh2D.VectorLength = (float)e.NewValue;
         SetMesh();
      }
      private void MI_ClearFiles_Click(object sender, RoutedEventArgs e)
      {
         L_AddElements.Content = "Elements file";
         L_AddVertices.Content = "Vertex file";
         L_AddResults.Content = "Result file";

         mesh?.RemoveMesh();
         mesh = null;
      }
      #endregion
      public void SetMesh()
      {
         mesh?.RemoveMesh();
         mesh = null;

         if (File.Exists(L_AddElements.Content as string) && File.Exists(L_AddVertices.Content as string) && File.Exists(L_AddResults.Content as string))
         {
            mesh = fieldType switch
            {
               FieldType.Scalar =>
                  new ScalarMesh2D(L_AddElements.Content as string,
                                   L_AddVertices.Content as string,
                                   L_AddResults.Content as string,
                                   (float)glControl.ActualWidth,
                                   (float)glControl.ActualHeight, meshType),
               FieldType.Vector =>
                   new VectorMesh2D(L_AddElements.Content as string,
                                   L_AddVertices.Content as string,
                                   L_AddResults.Content as string,
                                   (float)glControl.ActualWidth,
                                   (float)glControl.ActualHeight, ArrowType, meshType),
               _ => null
            };
         }
         else if (L_AddElements.Content as string != "File" && L_AddVertices.Content as string != "File" && mesh != null)
            if (!File.Exists(L_AddElements.Content as string))
               MessageBox.Show($"File \"{L_AddElements.Content}\" does not exist!");
            else if (!File.Exists(L_AddVertices.Content as string))
               MessageBox.Show($"File \"{L_AddVertices.Content}\" does not exist!");

      }

   }
}
