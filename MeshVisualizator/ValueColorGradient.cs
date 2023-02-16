using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Text;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace MeshVisualizator
{
   public class ColorKnot : INotifyPropertyChanged // model
   {
      private static readonly Regex _regexHex = new Regex(@"\b[\da-fA-F]+\b");
      private string colorCode = "FFFFFF";
      public bool CanGoDown { get => weight > 0f;}
      public bool CanGoUp { get => weight < 1f;}
      public string ColorCode 
      { get => colorCode; 
        set 
        {
           if (value.Length != 6 || !_regexHex.IsMatch(value))
              return;
           colorCode = value;
           OnPropertyChanged();
           OnPropertyChanged("RectColor");
           OnPropertyChanged("R");
           OnPropertyChanged("G");
           OnPropertyChanged("B");
      }}
      public string R
      { 
         get { return int.Parse(colorCode[0].ToString() + colorCode[1].ToString(), NumberStyles.AllowHexSpecifier, null).ToString(); } 
         set 
         {
            if (!uint.TryParse(value, out uint val))
               return;

            if (val > 255)
               return;

            StringBuilder sb = new(colorCode);
            colorCode = sb
                     .Remove(0, 2)
                     .Insert(0, int.Parse(value).ToString("X2"))
                     .ToString();
            OnPropertyChanged();
            OnPropertyChanged("ColorCode");
            OnPropertyChanged("RectColor");
         } 
      }
      public string G
      {
         get { return int.Parse(colorCode[2].ToString() + colorCode[3].ToString(), NumberStyles.AllowHexSpecifier, null).ToString(); }
         set
         {
            if (!uint.TryParse(value, out uint val))
               return;

            if (val > 255)
               return;

            StringBuilder sb = new(colorCode);
            colorCode = sb
                     .Remove(2, 2)
                     .Insert(2, int.Parse(value).ToString("X2"))
                     .ToString();
            OnPropertyChanged();
            OnPropertyChanged("ColorCode");
            OnPropertyChanged("RectColor");

         }
      }
      public string B
      {
         get { return int.Parse(colorCode[4].ToString() + colorCode[5].ToString(), NumberStyles.AllowHexSpecifier, null).ToString(); }
         set
         {
            if (!uint.TryParse(value, out uint val))
               return;

            if (val > 255)
               return;

            StringBuilder sb = new(colorCode);
            colorCode = sb
                     .Remove(4, 2)
                     .Insert(4, int.Parse(value).ToString("X2"))
                     .ToString();

            OnPropertyChanged();
            OnPropertyChanged("ColorCode");
            OnPropertyChanged("RectColor");

         }
      }

      private float weight;
      public float Weight 
      { get => weight;
         set
         {
            weight = value;
            OnPropertyChanged();

            // TODO: -> Update mesh, Update scale
        }}
      public Brush RectColor 
      { 
         get
         {
            Vector3 vcolor = GetColorVector();
            return new SolidColorBrush(Color.FromRgb((byte)MathF.Round(vcolor.X * 255),
                                                     (byte)MathF.Round(vcolor.Y * 255),
                                                     (byte)MathF.Round(vcolor.Z * 255)));

         }
      }
      
      public event PropertyChangedEventHandler? PropertyChanged;
      public void OnPropertyChanged([CallerMemberName] string prop = "") =>
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      public Vector3 GetColorVector()
      {
         int[] rgb = new int[3] { 0, 0, 0 };
         bool success = true;
         for (int i = 0; i < 3 && success; i++)
            success = int.TryParse(ColorCode[i * 2].ToString() + ColorCode[i * 2 + 1].ToString(), NumberStyles.AllowHexSpecifier, null, out rgb[i]);
         
         return new Vector3(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f);
      }
      public static string GetColorCodeByColorVector(Vector3 color)
      {
         return ((int)(color.X * 255)).ToString("X2") + 
                  ((int)(color.Y * 255)).ToString("X2") + 
                  ((int)(color.Z * 255)).ToString("X2");
      }
      public static bool IsStringAColor(string colorstring)
      {
         bool success = colorstring.Length == 6;
         for (int i = 0; i < 3 && success; i++)
            success = int.TryParse(new string(new char[] { colorstring[i * 2], colorstring[i * 2 + 1] }), NumberStyles.AllowHexSpecifier, null, out _);

         return success;
      }
   }

   public class ValueColorGradient // view-model
   {
      
      private ObservableCollection<ColorKnot> colorKnots;
      public IEnumerable<ColorKnot> ColorKnots => colorKnots;
      private float maxValue, minValue;
      private bool isLogScale = false;

      public float MaxValue { get => maxValue; set => maxValue = value; }
      public bool SetIsLogarithmic { set => isLogScale = value; }
      public float MinValue { get => minValue; set => minValue = value; }

      public ValueColorGradient(Vector3 minimalColor, Vector3 maximumColor, bool isScaleLogarithmic = false)
      {
         colorKnots = new ObservableCollection<ColorKnot>() 
            { new ColorKnot { Weight = 0f, ColorCode = ColorKnot.GetColorCodeByColorVector(minimalColor) },
              new ColorKnot { Weight = 1f, ColorCode = ColorKnot.GetColorCodeByColorVector(maximumColor) }};
         SetIsLogarithmic = isScaleLogarithmic;
      }
      public ValueColorGradient(string minimalColor, string maximumColor, bool isScaleLogarithmic = false)
      {
         if (!ColorKnot.IsStringAColor(minimalColor) || !ColorKnot.IsStringAColor(maximumColor))
            throw new Exception("minimalColor or maximumColor does not satisfy rgb hex format");
         colorKnots = new ObservableCollection<ColorKnot>()
         { new ColorKnot { Weight = 0f, ColorCode = minimalColor },
           new ColorKnot { Weight = 1f, ColorCode = maximumColor }};
         
            
         SetIsLogarithmic = isScaleLogarithmic;
      }
      public ValueColorGradient((Vector3 color, float w)[] Colors, float maxValue, float minValue, bool isScaleLogarithmic = false)
      {
         if (Colors.Where(x => x.w > 1f || x.w < 0f).ToArray().Length > 0)
            throw new ArgumentException("Weight must be in [0, 1]");
         colorKnots = new ObservableCollection<ColorKnot>();
         int id = 1;
         foreach (var color in Colors)
            colorKnots.Add(new ColorKnot { ColorCode = ColorKnot.GetColorCodeByColorVector(color.color), Weight = color.w});

         MaxValue = maxValue;
         MinValue = minValue;
         SetIsLogarithmic = isScaleLogarithmic;

      }
      public ValueColorGradient((Vector3 color, float w)[] Colors, bool isScaleLogarithmic = false)
      {
         if (Colors.Where(x => x.w > 1f || x.w < 0f).ToArray().Length > 0)
            throw new ArgumentException("Weight must be in [0, 1]");
         
         colorKnots = new ObservableCollection<ColorKnot>();
         int id = 1;
         foreach (var color in Colors)
            colorKnots.Add(new ColorKnot { ColorCode = ColorKnot.GetColorCodeByColorVector(color.color), Weight = color.w });

         SetIsLogarithmic = isScaleLogarithmic;

      }
      public ValueColorGradient(Vector3[] colors, bool isScaleLogarithmic = false)
      {
         colorKnots = new ObservableCollection<ColorKnot>();

         int id = 0;
         foreach (var color in colors)
            colorKnots.Add(new ColorKnot { ColorCode = ColorKnot.GetColorCodeByColorVector(color), Weight = (float)id++ / (colors.Length - 1f) });

         SetIsLogarithmic = isScaleLogarithmic;

      }
      public ValueColorGradient(bool isScaleLogarithmic = false)
      {
         colorKnots = new ObservableCollection<ColorKnot>
         {
            new ColorKnot { ColorCode = "FFFFFF", Weight = 0.5f }
         };
      }
      public void AddColorKnot(Vector3 color, float weight) 
      {
         if (weight > 1f || weight < 0f)
            throw new ArgumentException("Weight must be in [0, 1]");
         colorKnots.Add(new ColorKnot { Weight = weight, ColorCode = ColorKnot.GetColorCodeByColorVector(color) });
      }
      public Vector3 GetColorByValue(float value)
      {
         if (value < minValue || value > maxValue)
            return new Vector3(0f, 0f, 0f);

         float weight = isLogScale ?
              (value - MinValue) / (MaxValue - MinValue)
            : (value - MinValue) / (MaxValue - MinValue);
         return GetColorByWeight(weight);
      }
      public float GetValueByWeight(float weight)
      {
         if (weight < 0f || weight > 1f)
            throw new ArgumentException("Weight must be in [0, 1]");

         if (!isLogScale)
            return MathHelper.Lerp(minValue, maxValue, weight);

         float value = minValue;
         float s = maxValue - minValue,
               n = colorKnots.Count,
               b0 = s * (1f - 10f) / (1f - MathF.Pow(10f, n - 1)); // S = b0 * (1 - q^n) / (1 - q)
         value += MathF.Pow(10f, weight * (n - 1));

         return value;
      }
      public Vector3 GetColorByWeight(float weight)
      {
         if (weight > 1f || weight < 0f)
            throw new ArgumentException("Weight must be in [0, 1]");

         int n = -1;
         bool found = false;
         for (int i = colorKnots.Count - 2; i > 0 && !found; i--)
            if (found = colorKnots[i].Weight > weight) 
               n = i;

         float w_lerp = found ? MathHelper.Lerp(colorKnots[n + 1].Weight, colorKnots[n].Weight, weight) : 
                                MathHelper.Lerp(colorKnots[0].Weight, colorKnots[1].Weight, weight);

         return found ? new Vector3(colorKnots[n + 1].GetColorVector() + w_lerp * (colorKnots[n].GetColorVector() - colorKnots[n].GetColorVector())) :
                        new Vector3(colorKnots[0].GetColorVector() + w_lerp * (colorKnots[1].GetColorVector() - colorKnots[^2].GetColorVector()));
      }

      public void RemoveColorKnotByNumber(int n)
      {
         if (colorKnots.Count == 2)
            MessageBox.Show("You can't have less than 2 color nodes!", "Error!", MessageBoxButton.OK);
         else
            colorKnots.Remove(colorKnots[n]);
      }

      public void RemoveColorKnot(ColorKnot colorKnot)
      {
         if (colorKnots.Count == 2)
            MessageBox.Show("You can't have less than 2 color nodes!", "Error!", MessageBoxButton.OK);
         else
            colorKnots.Remove(colorKnot);
      }

      public void AddNewColorKnot()
      {
         colorKnots.Add(new ColorKnot { ColorCode = "ffffff", Weight = 0.5f });
         Sort();
      }

      internal void Sort()
      {
         var sortedCKs = colorKnots.OrderByDescending(x => x.Weight).ToArray();
         colorKnots.Clear();
         foreach (var ck in sortedCKs)
            colorKnots.Add(ck);
      }

      internal void MoveUp(ColorKnot colorKnot)
      {
         int i = Array.FindIndex(ColorKnots.ToArray(), x => x.Equals(colorKnot));
         if (i >= 0)
            (colorKnots[i - 1].Weight, colorKnots[i].Weight) = (colorKnots[i].Weight, colorKnots[i - 1].Weight);
         Sort();
      }

      internal void MoveDown(ColorKnot colorKnot)
      {
         int i = Array.FindIndex(ColorKnots.ToArray(), x => x.Equals(colorKnot));
         if (i >= 0)
            (colorKnots[i + 1].Weight, colorKnots[i].Weight) = (colorKnots[i].Weight, colorKnots[i + 1].Weight);
         Sort();
      }


   }
}
