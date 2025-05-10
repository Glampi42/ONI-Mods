using ErrandNotifier.Enums;
using ErrandNotifier.NotifiableErrandPacks;
using ErrandNotifier.Structs;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ErrandNotifier.NotificationsHierarchy {
   [SerializationConfig(MemberSerialization.OptIn)]
   public abstract class NotifiableErrand : KMonoBehaviour {
      public GNotification parentNotification;

      public abstract Workable Errand { get; }
      [Serialize]
      public Ref<KPrefabID> uiSymbolBearer;// the GameObject that the UISymbol will be displayed on (isn't always the GameObject that has the errand component: f.e. MoveTo errand)

      [Serialize]
      private int serializedNotificationID;
      [Serialize]
      private string serializedName;
      [Serialize]
      private string serializedTooltip;
      [Serialize]
      private GNotificationType serializedNType;
      [Serialize]
      private bool serializedPause;
      [Serialize]
      private bool serializedZoom;

      public override void OnPrefabInit() {
         base.OnPrefabInit();

         if(Errand == null)
         {
            Debug.LogWarning(Main.debugPrefix + "This NotifiableErrand component doesn't have a referenced errand; destroying the component.");
            Destroy(this);
         }
      }

      [OnDeserialized]
      public void OnDeserialized() {
         SerializationUtils.ReconstructNotification(serializedNotificationID, this, serializedName, serializedTooltip, serializedNType, serializedPause, serializedZoom);
      }

      [OnSerializing]
      public void OnSerializing() {
         serializedNotificationID = parentNotification?.notificationID ?? -1;

         if(parentNotification == null)// serializing other things isn't necessary
            return;

         serializedName = parentNotification.name;
         serializedTooltip = parentNotification.tooltip;
         serializedNType = parentNotification.type;
         serializedPause = parentNotification.pause;
         serializedZoom = parentNotification.zoom;
      }

      public override void OnCleanUp() {
         base.OnCleanUp();

         Remove(true, true, true);
      }

      public void UpdateUISymbol() {
         if(Main.notifierOverlay != default && (!uiSymbolBearer?.Get()?.IsNullOrDestroyed() ?? false))
         {
            Main.notifierOverlay.UpdateUISymbol(uiSymbolBearer.Get().gameObject, Errand, parentNotification);
         }
      }

      /// <summary>
      /// Disable this NotifiableErrand component.
      /// </summary>
      /// <param name="tryTriggerNotification">If true, the Notification will be attempted to be created (which will happen if the GNotification has no more errands)</param>
      /// <param name="removeUp">Shows the direction of the notification removal (is false if this NotifiableErrand is removed by the GNotification itself)</param>
      /// <param name="isBeingDestroyed">True if the component is about to be UnityEngine.Object.Destroy()ed</param>
      public void Remove(bool tryTriggerNotification, bool removeUp = true, bool isBeingDestroyed = false) {
         if(removeUp && parentNotification != null)
         {
            parentNotification.GetErrands().Remove(this);
            if(parentNotification.GetErrands().Count == 0)
            {
               Vector3 pos = uiSymbolBearer.Get()?.transform.position ?? this.transform.position;
               parentNotification.Remove(tryTriggerNotification, tryTriggerNotification ? new WorldPosition() { worldID = this.GetMyWorldId(), position = pos } : Utils.InvalidLocation);
            }
         }

         parentNotification = null;
         UpdateUISymbol();

         if(!isBeingDestroyed && !this.IsNullOrDestroyed())
         {
            uiSymbolBearer = null;
            enabled = false;
         }
      }
   }


   // have to do all this because MonoBehaviour components can't be generic (welp):
   [SerializationConfig(MemberSerialization.OptIn)]
   public class NotifiableErrand_Constructable : NotifiableErrand {
#pragma warning disable CS0649// warning about the field not being assigned a value (it is, by Unity)
      [MyCmpGet]
      private Constructable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class NotifiableErrand_Deconstructable : NotifiableErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Deconstructable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class NotifiableErrand_Diggable : NotifiableErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Diggable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class NotifiableErrand_Moppable : NotifiableErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Moppable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class NotifiableErrand_EmptyConduitWorkable : NotifiableErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private EmptyConduitWorkable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   public class NotifiableErrand_EmptySolidConduitWorkable : NotifiableErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private EmptySolidConduitWorkable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class NotifiableErrand_Movable : NotifiableErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Movable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
}
