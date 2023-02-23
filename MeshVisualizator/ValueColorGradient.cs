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
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MeshVisualizator
{
   public class ColorKnot : INotifyPropertyChanged // model
   {
      private static readonly Regex _regexHex = new Regex(@"[\da-fA-F]{6}");
      private string colorCode = "FFFFFF";
      public bool CanGoDown { get; set; }
      public bool CanGoUp { get; set; }

      public string ColorCode
      { get => colorCode; 
        set 
        {
           if (!_regexHex.IsMatch(value))
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
            if (value < 0f || value > 1f)
               return;
            weight = value;
            OnPropertyChanged();

            // TODO: -> Update mesh
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
         return new Vector3(int.Parse(R) / 255f, int.Parse(G) / 255f, int.Parse(B) / 255f);
      }
      public static string GetColorCodeByColorVector(Vector3 color)
      {
         return ((int)(color.X * 255)).ToString("X2") + 
                  ((int)(color.Y * 255)).ToString("X2") + 
                  ((int)(color.Z * 255)).ToString("X2");
      }
      public static bool IsStringAColor(string colorstring)
      {
         return _regexHex.IsMatch(colorstring);
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
         Sort();

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
         Sort();
      }
      public ValueColorGradient(Vector3[] colors, bool isScaleLogarithmic = false)
      {
         colorKnots = new ObservableCollection<ColorKnot>();

         int id = 0;
         foreach (var color in colors)
            colorKnots.Add(new ColorKnot { ColorCode = ColorKnot.GetColorCodeByColorVector(color), Weight = (float)id++ / (colors.Length - 1f) });

         SetIsLogarithmic = isScaleLogarithmic;
         Sort();
      }
      public ValueColorGradient(bool isScaleLogarithmic = false)
      {
         colorKnots = new ObservableCollection<ColorKnot>
         {
            new ColorKnot { ColorCode = "FFFFFF", Weight = 1f },
            new ColorKnot { ColorCode = "000000", Weight = 0f },
         };
         Sort();
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

         for (int i = 1; i < colorKnots.Count - 1 && !found; i++)
            if (found = colorKnots[i].Weight < weight) 
               n = i;
         
         float s = found ? colorKnots[n].Weight - colorKnots[n - 1].Weight:
                           colorKnots[^1].Weight - colorKnots[^2].Weight;
         float w_lerp = (weight - (found ? colorKnots[n - 1].Weight : colorKnots[^2].Weight)) / s;

         return found ? (1f - w_lerp ) * colorKnots[n - 1].GetColorVector() + w_lerp * colorKnots[n].GetColorVector() :
                        (1f -  w_lerp) * colorKnots[^2].GetColorVector() + w_lerp * colorKnots[^1].GetColorVector();
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
         colorKnots.Add(new ColorKnot { ColorCode = "f0f0f0", Weight = 0.5f });
         Sort();
      }
      internal void Sort()
      {
         var sortedCKs = colorKnots.OrderByDescending(x => x.Weight).ToArray();
         colorKnots.Clear();
         foreach (var ck in sortedCKs)
         {
            ck.CanGoDown = ck.CanGoUp = true;
            colorKnots.Add(ck);
         }
         sortedCKs.First().CanGoUp = false;
         sortedCKs.Last().CanGoDown = false;
      }
      internal void MoveUp(ColorKnot colorKnot)
      {
         int i = Array.FindIndex(ColorKnots.ToArray(), x => x.Equals(colorKnot));
         if (i >= 1)
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
      public void SetPalette(string palette_file)
      {
         if (!File.Exists(palette_file))
            return;

         using var palette_reader = new StreamReader(palette_file);
         if (!File.Exists(@"../../../Palette_schema.json"))
            throw new Exception($"Couldn't find schema \"Palette_schema.json\"");

         using var schema_reader = new StreamReader(@"../../../Palette_schema.json");

         try
         {
            JArray array = JArray.Parse(palette_reader.ReadToEnd());
            JSchema schema = JSchema.Parse(schema_reader.ReadToEnd());

            if (!array.IsValid(schema))
            {
               MessageBox.Show($"File {palette_file} is corrupted!", "Error!", MessageBoxButton.OK);
               return;
            }

            ColorKnot[] cks = new ColorKnot[array.Count];
            for (int i = 0; i < cks.Length; i++)
            {
               cks[i] = array[i].ToObject<ColorKnot>() ?? new ColorKnot();
            }

            colorKnots.Clear();
            foreach (var ck in cks)
               colorKnots.Add(ck);

            Sort();
         }
         catch 
         {
            MessageBox.Show($"File {palette_file} is corrupted!", "Error!", MessageBoxButton.OK);
            return;
         }
      }

      public void SavePalette(string palette_file)
      {
         if (palette_file == "")
            return;
         StringBuilder sb = new("[\n");

         foreach (var ck in colorKnots)
            sb.AppendLine($"{{\n\t\"ColorKnots\" : \"{ck.ColorCode}\",\n\t\"Weight\" : { ck.Weight.ToString(CultureInfo.InvariantCulture)} }},");
         sb.Append("]");

         File.WriteAllText(palette_file, sb.ToString());
      }
   }
}
