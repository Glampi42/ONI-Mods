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

      public abstract Workable Errand { get; protected set; }
      public Chore chore;
      [Serialize]
      public Ref<KPrefabID> chainNumberBearer;// the GameObject that the chain number will be displayed on (isn't always the GameObject that has the errand component: f.e. MoveTo errand)

      [Serialize]
      private int serializedChainID;
      [Serialize]
      private int serializedLinkNumber;
      [Serialize]
      private Color serializedChainColor;

      public void ConfigureChorePrecondition(Chore chore = null) {
         Debug.Log("ConfigureChorePrecondition for " + this.GetType().ToString());
         if(chore == null)
            chore = ChainedErrandPackRegistry.GetChainedErrandPack(this).GetChoreFromErrand(Errand);
         
         this.chore = chore;

         if(chore != null)
         {
            Debug.Log("AddPrecondition");
            if(!chore.preconditions.Any(p => p.id == Main.ChainedErrandPrecondition.id))
               chore.AddPrecondition(Main.ChainedErrandPrecondition);
            if(parentLink.linkNumber != 0)// stop dupes from doing errands that are not in the first link
               InterruptChore("Chore added to chain");
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
         Debug.Log("ChainedErrand.OnPrefabInit");
         base.OnPrefabInit();

         if(Main.IsGameLoaded)// won't run this code for buildings that get deserialized from a save file
         {
            // setting owner errand:
            if(!TrySetOwnerErrand())
            {
               Debug.LogWarning(Main.debugPrefix + "Couldn't find an owner errand for this ChainedErrand component; destroying the component.");
               UnityEngine.Object.Destroy(this);
            }
         }
      }

      [OnDeserialized]
      public void OnDeserialized() {
         Debug.Log("ChainedErrand.OnDeserialized");
         if(Errand == null)// this ChainedErrand component was added to its GameObject's prefab for the first time; have to set owner errand
         {
            if(!TrySetOwnerErrand())
            {
               Debug.LogWarning(Main.debugPrefix + "Couldn't find an owner errand for this deserialized ChainedErrand component; destroying the component.");
               UnityEngine.Object.Destroy(this);
            }
         }
         else// this ChainedErrand component was already there when the save file was created; have to recreate its chain
         {
            SerializationUtils.ReconstructChain(serializedChainID, serializedLinkNumber, this, serializedChainColor);
         }
      }

      [OnSerializing]
      public void OnSerializing() {
         serializedChainID = parentLink?.parentChain?.chainID ?? -1;
         serializedLinkNumber = parentLink?.linkNumber ?? -1;
         serializedChainColor = parentLink?.parentChain?.chainColor ?? Color.clear;
      }

      private bool TrySetOwnerErrand() {
         Debug.Log("TrySetOwnerErrand for " + this.GetType());
         Type errandType = ChainedErrandPackRegistry.GetChainedErrandPack(this).GetErrandType();

         Workable ownerErrand = (Workable)this.gameObject.GetComponent(errandType);
         if(ownerErrand != null)
         {
            Debug.Log("And its: " + ownerErrand.GetType());
            Errand = ownerErrand;
         }

         return Errand != null;
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
         Debug.Log("ChainedErrand.Remove");
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


   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Constructable : ChainedErrand {
      [Serialize]
      private Ref<Constructable> errand;
      // Ref<> needs to contain the actual type of the referenced errand. If it was Ref<Workable>, then the reference wouldn't be persistent
      //(for example pipe has both Deconstructable and EmptyConduitWorkable, Ref<Workable> would keep a reference to one of them, without the ability to differentiate)

      public override Workable Errand { get => errand?.Get(); protected set => errand = new Ref<Constructable>(value as Constructable); }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Deconstructable : ChainedErrand {
      [Serialize]
      private Ref<Deconstructable> errand;

      public override Workable Errand { get => errand?.Get(); protected set => errand = new Ref<Deconstructable>(value as Deconstructable); }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Diggable : ChainedErrand {
      [Serialize]
      private Ref<Diggable> errand;

      public override Workable Errand { get => errand?.Get(); protected set => errand = new Ref<Diggable>(value as Diggable); }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Moppable : ChainedErrand {
      [Serialize]
      private Ref<Moppable> errand;

      public override Workable Errand { get => errand?.Get(); protected set => errand = new Ref<Moppable>(value as Moppable); }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_EmptyConduitWorkable : ChainedErrand {
      [Serialize]
      private Ref<EmptyConduitWorkable> errand;

      public override Workable Errand { get => errand?.Get(); protected set => errand = new Ref<EmptyConduitWorkable>(value as EmptyConduitWorkable); }
   }
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand_Movable : ChainedErrand {
      [Serialize]
      private Ref<Movable> errand;

      public override Workable Errand { get => errand?.Get(); protected set => errand = new Ref<Movable>(value as Movable); }
   }
}