using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Strings {
   [HarmonyPatch(typeof(Localization), "Initialize")]
   public abstract class RegisterLocalizeStrings {
      private static List<System.Type> stringCollections;

      public static bool Prepare() {
         stringCollections = GetChildTypesOfType<RegisterLocalizeStrings>().ToList();
         return stringCollections.Count >= 1;

         IEnumerable<Type> GetChildTypesOfType<T>() => typeof(T).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(T)) && !x.IsAbstract).Select(x => x.GetNestedTypes()).SelectMany(x => x);
      }

      public static void Postfix() {
         System.Type type = stringCollections.FirstOrDefault();
         if(type == null)
            return;
         Localization.AddAssembly(type.Namespace, type.Assembly);

         string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "translations", Localization.GetLocaleForCode(Localization.GetCurrentLanguageCode())?.Code + ".po");
         if(File.Exists(path))
         {
            Localization.OverloadStrings(Localization.LoadStringsFile(path, false));
         }

         foreach(System.Type stringCollection in stringCollections)
         {
            LocString.CreateLocStringKeys(stringCollection);
         }
      }
   }
}
