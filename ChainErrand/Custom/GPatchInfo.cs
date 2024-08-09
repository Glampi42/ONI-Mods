using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand.Custom {
   public class GPatchInfo {
      public MethodInfo patchedMethod;
      public HarmonyMethod prefix;
      public HarmonyMethod postfix;
      public HarmonyMethod transpiler;

      public GPatchInfo(MethodInfo patchedMethod, MethodInfo prefix, MethodInfo postfix, MethodInfo transpiler = null) {
         this.patchedMethod = patchedMethod;
         this.prefix = prefix != null ? new HarmonyMethod(prefix) : null;
         this.postfix = postfix != null ? new HarmonyMethod(postfix) : null;
         this.transpiler = transpiler != null ? new HarmonyMethod(transpiler) : null;
      }
   }
}
