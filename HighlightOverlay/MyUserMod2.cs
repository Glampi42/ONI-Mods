using Epic.OnlineServices;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay {
   public class MyUserMod2 : UserMod2 {
      public override void OnLoad(Harmony harmony) {
         PUtil.InitLibrary(false);

         new POptions().RegisterOptions(this, typeof(ModConfig));

         base.OnLoad(harmony);
      }
   }
}
