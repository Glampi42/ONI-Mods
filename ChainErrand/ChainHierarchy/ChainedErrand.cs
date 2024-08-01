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

      [Serialize]
      public Ref<Workable> errand;
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
         Debug.Log("ConfigureChorePrecondition errand: " + (errand?.Get()?.GetType().ToString() ?? "NULL"));
         Debug.Log("This is " + this.GetType());
         if(chore == null)
            chore = ChainedErrandPackRegistry.GetChainedErrandPack(this).GetChoreFromErrand(errand?.Get());
         
         this.chore = chore;

         if(chore != null)
         {
            chore.AddPrecondition(Main.ChainedErrandPrecondition);
            if(parentLink.linkNumber != 0)// stop dupes from doing errands that are not in the first link
               chore.Fail("Chore added to chain");
         }
      }

      public override void OnPrefabInit() {
         base.OnPrefabInit();
         Debug.Log("ChainedErrand.OnPrefabInit, go name: " + (this.gameObject?.name ?? "NULL"));

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
         Debug.Log("type: " + (errand?.Get()?.GetType().ToString() ?? "NULL"));
         if(errand?.Get() == null)// this ChainedErrand component was added to its GameObject's prefab for the first time; have to set owner errand
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
         Debug.Log("ChainedErrand.OnSerializing");
         Debug.Log("type: " + (errand?.Get()?.GetType().ToString() ?? "NULL"));
         serializedChainID = parentLink?.parentChain?.chainID ?? -1;
         serializedLinkNumber = parentLink?.linkNumber ?? -1;
         serializedChainColor = parentLink?.parentChain?.chainColor ?? Color.clear;
      }
      [OnSerialized]
      public void OnSerializedDebug() {
         Debug.Log("ChainedErrand.OnSerialized");
         Debug.Log("type: " + (errand?.Get()?.GetType().ToString() ?? "NULL"));
      }
      [OnDeserializing]
      public void OnDeserializingDebug() {
         Debug.Log("ChainedErrand.OnDeserializing");
         Debug.Log("type: " + (errand?.Get()?.GetType().ToString() ?? "NULL"));
      }

      private bool TrySetOwnerErrand() {
         Debug.Log("TrySetOwnerErrand for " + this.GetType());
         Type errandType = ChainedErrandPackRegistry.GetChainedErrandPack(this).GetErrandType();

         Workable ownerErrand = (Workable)this.gameObject.GetComponent(errandType);
         if(ownerErrand != null)
         {
            Debug.Log("And its: " + ownerErrand.GetType());
            errand = new Ref<Workable>(ownerErrand);
         }

         return errand?.Get() != null;
      }

      public override void OnCleanUp() {
         base.OnCleanUp();

         Remove(true, true);
      }

      public void UpdateChainNumber() {
         if(Main.chainOverlay != default)
         {
            Debug.Log("UpdateChainNumber, bearer not null: " + (chainNumberBearer?.Get()?.gameObject != null));
            Main.chainOverlay.UpdateChainNumber(chainNumberBearer?.Get()?.gameObject, errand?.Get(), parentLink);
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

            Debug.Log("QYBS: " + this.IsNullOrDestroyed());
            this.enabled = false;
         }
      }
   }


   public class ChainedErrand_Constructable : ChainedErrand { }
   public class ChainedErrand_Deconstructable : ChainedErrand { }
   public class ChainedErrand_Diggable : ChainedErrand { }
   public class ChainedErrand_Moppable : ChainedErrand { }
   public class ChainedErrand_EmptyConduitWorkable : ChainedErrand { }
   public class ChainedErrand_Movable : ChainedErrand { }
}