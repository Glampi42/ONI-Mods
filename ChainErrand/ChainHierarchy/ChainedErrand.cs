using ChainErrand.ChainedErrandPacks;
using ChainErrand.Patches;
using ChainErrand.Strings;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace ChainErrand.ChainHierarchy {
   [SerializationConfig(MemberSerialization.OptIn)]
   public abstract class ChainedErrand : KMonoBehaviour {
      public Link parentLink;

      public abstract Workable Errand { get; }
      public Chore chore;
      [Serialize]
      public Ref<KPrefabID> chainNumberBearer;// the GameObject that the ChainNumber will be displayed on (isn't always the GameObject that has the errand component: f.e. MoveTo errand)

      [Serialize]
      private int serializedChainID;
      [Serialize]
      private int serializedLinkNumber;
      [Serialize]
      private Color serializedChainColor;

      public void ConfigureChorePrecondition(Chore chore = null) {
         if(chore == null)
            chore = ChainedErrandPackRegistry.GetChainedErrandPack(this).GetChoreFromErrand(Errand);
         
         this.chore = chore;

         if(chore != null)
         {
            if(!chore.preconditions.Any(p => p.id == Main.ChainedErrandPrecondition.id))
               chore.AddPrecondition(Main.ChainedErrandPrecondition);
            if(parentLink.linkNumber != 0)// stop dupes from doing errands that are not in the first link
               InterruptChore("Chore was added to a chain");
         }
      }

      /// <summary>
      /// Stops the execution of the chore related to this errand.
      /// </summary>
      /// <param name="reason">The reason it was interrupted</param>
      public void InterruptChore(string reason) {
         chore?.Fail(reason);
      }

      public override void OnPrefabInit() {
         base.OnPrefabInit();

         if(Errand == null)
         {
            Debug.LogWarning(Main.debugPrefix + "This ChainedErrand component doesn't have a referenced errand; destroying the component.");
            UnityEngine.Object.Destroy(this);
         }
      }

      [OnDeserialized]
      public void OnDeserialized() {
         SerializationUtils.ReconstructChain(serializedChainID, serializedLinkNumber, this, serializedChainColor);
      }

      [OnSerializing]
      public void OnSerializing() {
         serializedChainID = parentLink?.parentChain?.chainID ?? -1;
         serializedLinkNumber = parentLink?.linkNumber ?? -1;
         serializedChainColor = parentLink?.parentChain?.chainColor ?? Color.clear;
      }

      public override void OnCleanUp() {
         base.OnCleanUp();

         Remove(true, true);
      }

      public void UpdateChainNumber() {
         if(Main.chainOverlay != default)
         {
            Main.chainOverlay.UpdateChainNumber(chainNumberBearer?.Get()?.gameObject, Errand, parentLink);
         }
      }

      public void Remove(bool tryRemoveLink, bool isBeingDestroyed = false) {
         if(tryRemoveLink && parentLink != null)
         {
            parentLink.errands.Remove(this);
            if(parentLink.errands.Count == 0)
            {
               parentLink.Remove(true);
            }
         }

         parentLink = null;
         UpdateChainNumber();

         if(!isBeingDestroyed)
         {
            // removing the chore precondition:
            if(chore != null)
            {
               chore.preconditions.Remove(chore.preconditions.FirstOrDefault(precondition => precondition.id == nameof(Main.ChainedErrandPrecondition)));
            }

            chore = null;
            chainNumberBearer = null;

            this.enabled = false;
         }
      }
   }


   // have to do all this because MonoBehaviour components can't be generic (welp):
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Constructable : ChainedErrand {
#pragma warning disable CS0649// warning about the field not being assigned a value (it is, by unity)
      [MyCmpGet]
      private Constructable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Deconstructable : ChainedErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Deconstructable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Diggable : ChainedErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Diggable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Moppable : ChainedErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Moppable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_EmptyConduitWorkable : ChainedErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private EmptyConduitWorkable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Movable : ChainedErrand {
#pragma warning disable CS0649
      [MyCmpGet]
      private Movable errand;
#pragma warning restore CS0649

      public override Workable Errand { get => errand; }
   }
}