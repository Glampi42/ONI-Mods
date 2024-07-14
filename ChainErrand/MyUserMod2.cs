using HarmonyLib;
using KMod;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand {
   public class MyUserMod2 : UserMod2 {
      public override void OnLoad(Harmony harmony) {
         base.OnLoad(harmony);
         PUtil.InitLibrary();
         Main.chainTool_binding = new PActionManager().CreateAction("glampi.ChainTool", (LocString)"ChainTool", new PKeyBinding());
      }
   }
}
