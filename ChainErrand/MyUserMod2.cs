﻿using ChainErrand.ChainedErrandPacks;
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
         new POptions().RegisterOptions(this, typeof(ModConfig));

         // patching the pathes defined in ChainedErrandPacks:
         foreach(var pack in ChainedErrandPackRegistry.AllPacks())
         {
            var createPatches = pack.OnChoreCreate_Patch();
            if(createPatches != null)
            {
               foreach(var patch in createPatches)
               {
                  harmony.Patch(patch.patchedMethod, patch.prefix, patch.postfix);
               }
            }

            var deletePatches = pack.OnChoreDelete_Patch();
            if(deletePatches != null)
            {
               foreach(var patch in deletePatches)
               {
                  harmony.Patch(patch.patchedMethod, patch.prefix, patch.postfix);
               }
            }
         }
      }
   }
}
