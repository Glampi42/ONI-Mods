using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand {
   public static class Utils {
      /// <summary>
      /// Generates a random color that is constrained by the specified parameters.
      /// </summary>
      /// <param name="minLuminocity">Minimal brightness of the color; from 0: black to 1: white</param>
      /// <param name="maxLuminocity">Maximal brightness of the color; from 0: black to 1: white</param>
      /// <param name="minSaturation">Minimal saturation of the color; from 0: unsaturated to 1: saturated</param>
      /// <param name="maxSaturation">Maximal saturation of the color; from 0: unsaturated to 1: saturated</param>
      /// <param name="r">Random number generator</param>
      /// <returns>The generated color.</returns>
      public static Color RandomColor(double minLuminocity, double maxLuminocity, double minSaturation, double maxSaturation, System.Random r) {
         if(minLuminocity < 0d || minLuminocity > 1d)
            throw new ArgumentOutOfRangeException(nameof(minLuminocity));
         if(maxLuminocity < 0d || maxLuminocity > 1d)
            throw new ArgumentOutOfRangeException(nameof(maxLuminocity));
         if(minSaturation < 0d || minSaturation > 1d)
            throw new ArgumentOutOfRangeException(nameof(minSaturation));
         if(maxSaturation < 0d || maxSaturation > 1d)
            throw new ArgumentOutOfRangeException(nameof(maxSaturation));

         minLuminocity = Math.Min(minLuminocity, maxLuminocity);
         maxLuminocity = Math.Max(minLuminocity, maxLuminocity);
         minSaturation = Math.Min(minSaturation, maxSaturation);
         maxSaturation = Math.Max(minSaturation, maxSaturation);// just for safety man

         double randomHue = r.NextDouble() * 360;
         double saturation = minSaturation + r.NextDouble() * (maxSaturation - minSaturation);
         double luminocity = minLuminocity + r.NextDouble() * (maxLuminocity - minLuminocity);
         return ColorFromHSL(randomHue, saturation, luminocity);
      }
      private static Color ColorFromHSL(double h, double s, double l) {
         if(s == 0)
         {
            return new Color((float)l, (float)l, (float)l, 1f);
         }

         double min, max, hue;
         hue = h / 360d;

         max = l < 0.5d ? l * (1 + s) : (l + s) - (l * s);
         min = (l * 2d) - max;

         Color c = new Color((float)RGBChannelFromHue(min, max, hue + 0.333333), (float)RGBChannelFromHue(min, max, hue), (float)RGBChannelFromHue(min, max, hue - 0.333333), 1f);
         return c;
      }
      private static double RGBChannelFromHue(double m1, double m2, double h) {
         h = (h + 1d) % 1d;
         if(h < 0)
            h += 1;
         if(h * 6 < 1)
            return m1 + (m2 - m1) * 6 * h;
         else if(h * 2 < 1)
            return m2;
         else if(h * 3 < 2)
            return m1 + (m2 - m1) * 6 * (2d / 3d - h);
         else
            return m1;
      }

      public static float GetFontSizeFromLinkNumber(int linkNum) {
         // exponential function that goes from maxSize at x = 0 to minSize at x = +infinity
         return Math.Max((float)(Math.Exp(Main.chainNumberDecreaseRate * -linkNum) * (Main.maxChainNumberFontSize - Main.minChainNumberFontSize) + Main.minChainNumberFontSize), Main.minChainNumberFontSize);
      }


      public static ToolTip AddSimpleTooltip(this GameObject go, string tooltip, bool alignCenter = true, float wrapWidth = 0, bool onBottom = true) {
         if(go == null)
            return null;

         var tooltipCmp = go.AddOrGet<ToolTip>();
         tooltipCmp.UseFixedStringKey = false;
         tooltipCmp.enabled = true;
         tooltipCmp.tooltipPivot = alignCenter ? new Vector2(0.5f, onBottom ? 1f : 0f) : new Vector2(1f, onBottom ? 1f : 0f);
         tooltipCmp.tooltipPositionOffset = onBottom ? new Vector2(0f, -20f) : new Vector2(0f, 20f);
         tooltipCmp.parentPositionAnchor = new Vector2(0.5f, 0.5f);
         if(wrapWidth > 0)
         {
            tooltipCmp.WrapWidth = wrapWidth;
            tooltipCmp.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
         }
         //ToolTipScreen.Instance.SetToolTip(tooltipCmp);
         tooltipCmp.SetSimpleTooltip(tooltip);
         return tooltipCmp;
      }

      public static void SaveSpriteToAssets(string sprite_name, string additional_path = null) {
         Texture2D texture = LoadTexture(sprite_name, additional_path);
         Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector3.zero);
         sprite.name = sprite_name;
         Assets.Sprites.Add(sprite_name, sprite);
      }
      private static Texture2D LoadTexture(string name, string additional_path) {
         Texture2D texture = null;
         string path = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets" + (additional_path ?? "")), name + ".png");
         try
         {
            byte[] data = File.ReadAllBytes(path);
            texture = new Texture2D(1, 1);
            texture.LoadImage(data);
         }
         catch(Exception ex)
         {
            Debug.LogError((object)(Main.debugPrefix + "Could not load texture at " + path));
            Debug.LogException(ex);
         }
         return texture;
      }
      //---------------------Extensions---------------------DOWN
      public static Vector3 InverseLocalScale(this RectTransform rectTransform) {
         Vector3 localScale = rectTransform.localScale;
         return new Vector3(1f / localScale.x, 1f / localScale.y, 1f / localScale.z);
      }

      /// <returns>The transform child of the specified index or null if such child doesn't exist</returns>
      public static Transform GetChildSafe(this Transform parent, int index) {
         if(parent.childCount <= index)
            return null;

         return parent.GetChild(index);
      }

      public static Transform FindRecursively(this Transform parent, string childName) {
         if(parent.childCount == 0)
            return null;

         for(int i = 0; i < parent.childCount; i++)
         {
            var child = parent.GetChild(i);
            if(child.name == childName)
               return child;

            if((child = FindRecursively(child, childName)) != null)
               return child;
         }

         return null;
      }

      public static T GetOrDefault<K, T>(this Dictionary<K, T> dict, K key) {
         if(dict.ContainsKey(key))
            return dict[key];

         return default;
      }
      //---------------------Extensions---------------------UP
   }
}
