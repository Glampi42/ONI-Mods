using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Components {
   [SerializationConfig(MemberSerialization.OptIn)]
   public abstract class NotifiableErrand : KMonoBehaviour {
      public Notification parentNotification;

      public abstract Workable Errand { get; }
      public Chore chore;
      [Serialize]
      public Ref<KPrefabID> uiSymbolBearer;// the GameObject that the UISymbol will be displayed on (isn't always the GameObject that has the errand component: f.e. MoveTo errand)

      [Serialize]
      private int serializedNotificationID;

      public override void OnPrefabInit() {
         base.OnPrefabInit();

         if(Errand == null)
         {
            Debug.LogWarning(Main.debugPrefix + "This NotifiableErrand component doesn't have a referenced errand; destroying the component.");
            UnityEngine.Object.Destroy(this);
         }
      }

      [OnDeserialized]
      public void OnDeserialized() {
         //SerializationUtils.ReconstructChain(serializedNotificationID, serializedLinkNumber, this, serializedChainColor);
      }

      [OnSerializing]
      public void OnSerializing() {
         //serializedNotificationID = parentLink?.parentChain?.chainID ?? -1;
         //serializedLinkNumber = parentLink?.linkNumber ?? -1;
         //serializedChainColor = parentLink?.parentChain?.chainColor ?? Color.clear;
      }

      public override void OnCleanUp() {
         base.OnCleanUp();

         Remove(true, true);
      }

      public void UpdateUISymbol() {
         if(Main.notifierOverlay != default && (!uiSymbolBearer?.Get()?.IsNullOrDestroyed() ?? false))
         {
            //Main.chainOverlay.UpdateChainNumber(uiSymbolBearer.Get().gameObject, Errand, parentLink);
         }
      }

      public void Remove(bool tryRemoveLink, bool isBeingDestroyed = false) {
         //if(tryRemoveLink && parentLink != null)
         //{
         //   parentLink.errands.Remove(this);
         //   if(parentLink.errands.Count == 0)
         //   {
         //      parentLink.Remove(true);
         //   }
         //}

         //parentLink = null;
         UpdateUISymbol();

         if(!isBeingDestroyed && !this.IsNullOrDestroyed())
         {
            // disabling the chore precondition:
            if(chore != null)
            {
               //var precondition = chore.GetPreconditions().FirstOrDefault(precondition => precondition.condition.id == nameof(Main.NotifiableErrandPrecondition));
               //if(precondition.condition.id != default)
               //   precondition.data = false;
            }

            chore = null;
            uiSymbolBearer = null;

            this.enabled = false;
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
