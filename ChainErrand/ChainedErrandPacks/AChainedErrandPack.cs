using ChainErrand.ChainHierarchy;
using ChainErrand.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainedErrandPacks {
   /// <summary>
   /// This interface contains methods that any ChainedErrandPack must include. Each ChainedErrandPack is responsible for one errand type
   /// and does (nearly) everything necessary so that all errands of that type can be added to a chain.
   /// </summary>
   public interface IChainedErrandPack {
      public Type GetErrandType();
      public Type GetChainedErrandType();

      /// <summary>
      /// The patches that should happen whenever a chore related to this errand is created.
      /// </summary>
      public abstract List<GPatchInfo> OnChoreCreate_Patch();

      /// <summary>
      /// The patches that should happen whenever a chore related to this errand is deleted.
      /// </summary>
      public abstract List<GPatchInfo> OnChoreDelete_Patch();

      /// <summary>
      /// The patches that add a newly created errand to a chain automatically.
      /// </summary>
      public abstract List<GPatchInfo> OnAutoChain_Patch();

      /// <summary>
      /// Collects errands of type ErrandType that may be added to a chain.
      /// </summary>
      /// <param name="gameObject">The GameObject the errands will be collected from</param>
      /// <param name="errands">The HashSet containing the collected errands</param>
      /// <param name="errandReference">Component attached to the GameObject that represents the errand (is used in ChainOverlay)</param>
      /// <returns>True if at least one errand was collected; false otherwise.</returns>
      public abstract bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference);

      /// <summary>
      /// Retrieves the chore related to the specified errand, or null if it couldn't be found.
      /// </summary>
      /// <param name="errand">The errand</param>
      /// <returns>The chore.</returns>
      public abstract Chore GetChoreFromErrand(object errand);
   }

   /// <summary>
   /// This intermediate abstract class adds the two types that each ChainedErrandPack must define.
   /// </summary>
   /// <typeparam name="ErrandType">Type of the errand the ChainedErrandPack is responsible for</typeparam>
   /// <typeparam name="ChainedErrandType">Type of the ChainedErrand the ChainedErrandPack is responsible for</typeparam>
   public abstract class AChainedErrandPack<ErrandType, ChainedErrandType> : IChainedErrandPack
      where ErrandType : Workable
      where ChainedErrandType : ChainedErrand {
      public Type GetErrandType() => typeof(ErrandType);
      public Type GetChainedErrandType() => typeof(ChainedErrandType);

      public abstract List<GPatchInfo> OnChoreCreate_Patch();

      public abstract List<GPatchInfo> OnChoreDelete_Patch();

      public abstract List<GPatchInfo> OnAutoChain_Patch();

      public abstract bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference);

      public abstract Chore GetChoreFromErrand(ErrandType errand);
      public Chore GetChoreFromErrand(object errand) => GetChoreFromErrand(errand as ErrandType);
   }
}
