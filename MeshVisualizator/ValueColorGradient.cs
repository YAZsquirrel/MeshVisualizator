using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OpenTK.Mathematics;

namespace MeshVisualizator
{
   public class ValueColorGradient
   {
      public List<(Vector3 color, float w)> ColorKnots;
      private float maxValue, minValue;

      public float MaxValue { get => maxValue; set => maxValue = value; }
      public float MinValue { get => minValue; set => minValue = value; }

      public ValueColorGradient (Vector3 minimalColor, Vector3 maximumColor)
      {
         ColorKnots = new List<(Vector3, float)>() { (minimalColor, 0f), (maximumColor, 1f)};
      }
      public ValueColorGradient((Vector3 colors, float w)[] Colors, float maxValue, float minValue)
      {
         if (Colors.Where(x => x.w > 1f || x.w < 0f).ToArray().Length > 0)
            throw new ArgumentException("Weight must be in [0, 1]");
         
         ColorKnots = Colors.ToList();
         MaxValue = maxValue;
         MinValue = minValue;
      }
      public ValueColorGradient((Vector3 colors, float w)[] Colors)
      {
         if (Colors.Where(x => x.w > 1f || x.w < 0f).ToArray().Length > 0)
            throw new ArgumentException("Weight must be in [0, 1]");

         ColorKnots = Colors.ToList();
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
         float weight = MathHelper.Lerp(MinValue, MaxValue, value);// (value - MinValue) / (MaxValue - MinValue);
         return GetColorByWeight(weight);
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
      void RemoveColorKnotByNumber(int n)
      {
         if (ColorKnots.Count == 2)
            MessageBox.Show("You can't have less than 2 color nodes!", "Error!", MessageBoxButton.OK);
         else
            ColorKnots.Remove(ColorKnots[n]);
      }


   }
}
