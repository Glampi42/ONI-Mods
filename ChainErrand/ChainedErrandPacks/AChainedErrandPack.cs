using ChainErrand.ChainHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   /// <summary>
   /// This class contains methods, classes etc. that any ChainedErrandPack must include. Each ChainedErrandPack is responsible for one errand type
   /// and does (nearly) everything necessary so that all errands of that type can be added to a chain.
   /// </summary>
   public abstract class AChainedErrandPack<ErrandType, ChainedErrandType>
      where ErrandType : Workable
      where ChainedErrandType : ChainedErrand {
      public static Type GetErrandType() => typeof(ErrandType);
      public static Type GetChainedErrandType() => typeof(ChainedErrandType);

      /// <summary>
      /// The patch that should happen whenever a chore related to this errand is created.
      /// </summary>
      public abstract class OnChoreCreate_Patch;
      /// <summary>
      /// The patch that should happen whenever a chore related to this errand is deleted.
      /// </summary>
      public abstract class OnChoreDelete_Patch;

      /// <summary>
      /// Collects errands of type ErrandType from the specified GameObject.
      /// </summary>
      /// <param name="gameObject">The GameObject</param>
      /// <param name="errandReference">Component attached to the GameObject that represents the errand (is used in ChainOverlay)</param>
      public abstract HashSet<Workable> CollectErrands(GameObject gameObject, ref KMonoBehaviour errandReference);

      /// <summary>
      /// Retrieves the chore related to the specified errand, or null if it couldn't be found.
      /// </summary>
      /// <param name="errand">The errand</param>
      /// <returns>The chore.</returns>
      public abstract Chore GetChoreFromErrand(ErrandType errand);
   }
}
