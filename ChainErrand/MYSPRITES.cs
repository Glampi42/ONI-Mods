using ProcGenGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static STRINGS.DUPLICANTS.ATTRIBUTES;

namespace ChainErrand {
   public class MYSPRITES {
      private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

      public static Sprite GetSprite(string spriteName) {
         if(!sprites.ContainsKey(spriteName))
            throw new ArgumentException(Main.debugPrefix + $"Sprite \"{spriteName}\" was not found in the dictionary");

         return sprites[spriteName];
      }

      public static void SaveSprite(string sprite_name, string additional_path = null) {
         Texture2D texture = LoadTexture(sprite_name, additional_path);
         Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
         sprite.name = sprite_name;
         sprites.Add(sprite_name, sprite);
      }
      private static Texture2D LoadTexture(string name, string additional_path) {
         Texture2D texture = null;
         string path = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets" + (additional_path ?? "")), name + ".png");
         try
         {
            byte[] data = File.ReadAllBytes(path);
            texture = new Texture2D(2, 2);
            texture.LoadImage(data, false);
         }
         catch(Exception ex)
         {
            Debug.LogError((Main.debugPrefix + "Could not load texture at " + path));
            Debug.LogException(ex);
         }
         return texture;
      }
   }
}
