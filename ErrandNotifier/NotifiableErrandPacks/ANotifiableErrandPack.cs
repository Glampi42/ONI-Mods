using ErrandNotifier.Custom;
using ErrandNotifier.NotificationsHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotifiableErrandPacks {
   /// <summary>
   /// This interface contains methods that any NotifiableErrandPack must include. Each NotifiableErrandPack is responsible for one errand type
   /// and does (nearly) everything necessary so that all errands of that type can be added to a notification.
   /// </summary>
   public interface INotifiableErrandPack {
      public Type GetErrandType();
      public Type GetNotifiableErrandType();

      /// <summary>
      /// The patches that should happen whenever a chore related to this errand is deleted/finished.
      /// </summary>
      public abstract List<GPatchInfo> OnChoreDelete_Patch();

      /// <summary>
      /// Collects errands of type ErrandType that may be added to a notification.
      /// </summary>
      /// <param name="gameObject">The GameObject the errands will be collected from</param>
      /// <param name="errands">The HashSet containing the collected errands</param>
      /// <param name="errandReference">Component attached to the GameObject that represents the errand (is used in NotifierOverlay)</param>
      /// <returns>True if at least one errand was collected; false otherwise.</returns>
      public abstract bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference);
   }

   /// <summary>
   /// This intermediate abstract class adds the two types that each NotifiableErrandPack must define.
   /// </summary>
   /// <typeparam name="ErrandType">Type of the errand the NotifiableErrandPack is responsible for</typeparam>
   /// <typeparam name="NotifiableErrandType">Type of the NotifiableErrand the NotifiableErrandPack is responsible for</typeparam>
   public abstract class ANotifiableErrandPack<ErrandType, NotifiableErrandType> : INotifiableErrandPack
      where ErrandType : Workable
      where NotifiableErrandType : NotifiableErrand {
      public Type GetErrandType() => typeof(ErrandType);
      public Type GetNotifiableErrandType() => typeof(NotifiableErrandType);

      public abstract List<GPatchInfo> OnChoreDelete_Patch();

      public abstract bool CollectErrands(GameObject gameObject, HashSet<Workable> errands, ref KMonoBehaviour errandReference);
   }
}
