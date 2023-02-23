using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeshVisualizator
{
    /// <summary>
    /// Логика взаимодействия для ScaleColorsControl.xaml
    /// </summary>
   public partial class ScaleColorsControl : UserControl
   {
      MainWindow ParentWindow;
      public ScaleColorsControl()
      {
         InitializeComponent();
      }
      private void B_Remove_Click(object sender, RoutedEventArgs e)
      {
         ParentWindow.vcg.RemoveColorKnot(ParentWindow.vcg.ColorKnots
                                          .First(x => x.Weight == float.Parse(TBox_Weight.Text, CultureInfo.InvariantCulture) 
                                                   && x.ColorCode == TBox_ColorCode.Text));
         ParentWindow.DrawScale();
         ParentWindow.DrawSolution();
      }

      private void B_MoveUp_Click(object sender, RoutedEventArgs e)
      {
         ParentWindow?.vcg.MoveUp(ParentWindow.vcg.ColorKnots
                                          .First(x => x.Weight == float.Parse(TBox_Weight.Text, CultureInfo.InvariantCulture)
                                                   && x.ColorCode == TBox_ColorCode.Text));
         ParentWindow.DrawScale();
         ParentWindow.DrawSolution();
      }
      
      private void B_MoveDown_Click(object sender, RoutedEventArgs e)
      {
         ParentWindow?.vcg.MoveDown(ParentWindow.vcg.ColorKnots
                                          .First(x => x.Weight == float.Parse(TBox_Weight.Text, CultureInfo.InvariantCulture)
                                                   && x.ColorCode == TBox_ColorCode.Text));
         ParentWindow.DrawScale();
         ParentWindow.DrawSolution();
      }
      private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         ParentWindow?.DrawScale();
         ParentWindow?.DrawSolution();
         if (Mouse.LeftButton == MouseButtonState.Released)
            ParentWindow?.vcg.Sort();

      }
      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         ParentWindow = (MainWindow)Window.GetWindow(this);
      }

      private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
      {
         if (e.LeftButton != MouseButtonState.Pressed)
            ParentWindow?.vcg.Sort();

      }
      private void TBox_ColorCode_LostFocus(object sender, RoutedEventArgs e)
      {
         ParentWindow?.DrawScale();
         ParentWindow?.DrawSolution();
      }
   }
}
