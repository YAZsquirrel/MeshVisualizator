using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MeshVisualizator
{
   public class ValueColorGradient
   {
      public List<(Vector3 color, float w)> ColorKnots;
      private float maxValue, minValue;
      private bool isLogScale = false;

      public float MaxValue { get => maxValue; set => maxValue = value; }
      public bool SetIsLogarithmic { set => isLogScale = value; }
      public float MinValue { get => minValue; set => minValue = value; }

      public ValueColorGradient(Vector3 minimalColor, Vector3 maximumColor, bool isScaleLogarithmic = false)
      {
         ColorKnots = new List<(Vector3, float)>() { (minimalColor, 0f), (maximumColor, 1f)};
         SetIsLogarithmic = isScaleLogarithmic;
      }
      public ValueColorGradient((Vector3 colors, float w)[] Colors, float maxValue, float minValue, bool isScaleLogarithmic = false)
      {
         if (Colors.Where(x => x.w > 1f || x.w < 0f).ToArray().Length > 0)
            throw new ArgumentException("Weight must be in [0, 1]");
         
         ColorKnots = Colors.ToList();
         MaxValue = maxValue;
         MinValue = minValue;
         SetIsLogarithmic = isScaleLogarithmic;

      }
      public ValueColorGradient((Vector3 colors, float w)[] Colors, bool isScaleLogarithmic = false)
      {
         if (Colors.Where(x => x.w > 1f || x.w < 0f).ToArray().Length > 0)
            throw new ArgumentException("Weight must be in [0, 1]");

         ColorKnots = Colors.ToList();
         SetIsLogarithmic = isScaleLogarithmic;

      }
      public ValueColorGradient(Vector3[] colors, bool isScaleLogarithmic = false)
      {
         ColorKnots = new List<(Vector3 color, float w)>();

         for (int i = 0; i < colors.Length; i++)
            ColorKnots.Add((colors[i], (float)i / (colors.Length - 1)));
         SetIsLogarithmic = isScaleLogarithmic;

      }

      public void AddColorKnot(Vector3 color, float weight) 
      {
         if (weight > 1f || weight < 0f)
            throw new ArgumentException("Weight must be in [0, 1]");
         ColorKnots.Add((color, weight));
         ColorKnots.OrderBy(x => x.w).ToList();
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
               n = ColorKnots.Count,
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
         for (int i = 1; i < ColorKnots.Count - 1 && !found; i++)
            if (found = ColorKnots[i].w > weight) 
               n = i - 1;

         float w_lerp = found ? MathHelper.Lerp(ColorKnots[n].w, ColorKnots[n + 1].w, weight) : 
                                MathHelper.Lerp(ColorKnots[^2].w, ColorKnots[^1].w, weight);

         return found ? new Vector3(ColorKnots[n].color + w_lerp * (ColorKnots[n + 1].color - ColorKnots[n].color)) :
                        new Vector3(ColorKnots[^2].color + w_lerp * (ColorKnots[^1].color - ColorKnots[^2].color));
      }

      public void RemoveColorKnotByNumber(int n)
      {
         if (ColorKnots.Count == 2)
            MessageBox.Show("You can't have less than 2 color nodes!", "Error!", MessageBoxButton.OK);
         else
            ColorKnots.Remove(ColorKnots[n]);
      }


   }
}
