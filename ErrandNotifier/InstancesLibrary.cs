using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier {
   /// <summary>
   /// A library that contains fields with various types that are needed for Reflection.
   /// </summary>
   public static class InstancesLibrary {
      public static System.Action<Movable> Action_Movable = default;
      public static List<Ref<Movable>> List_Movable = default;
   }
}
