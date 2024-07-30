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
         if(chore == null)
            chore = Utils.GetChoreFromErrand(errand?.Get());
         
         this.chore = chore;

         if(chore != null)
         {
            chore.AddPrecondition(Main.ChainedErrandPrecondition);
            if(parentLink.linkNumber != 0)// stop dupes from doing errands that are not in the first link
               chore.Fail("Chore added to chain");
         }
      }

      public override void OnPrefabInit() {// runs when an instance of ChainedErrand gets created (despite its name)
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
         if(errand?.Get() == null)// this ChainedErrand component was added to its GameObject's prefab for the first time; have to set owner errand
         {
            if(!TrySetOwnerErrand())
            {
               Debug.LogWarning(Main.debugPrefix + "Couldn't find an owner errand for this deserialized ChainedErrand component; destroying the component.");
               UnityEngine.Object.Destroy(this);
            }
         }
         else// this ChainedErrand component was already there when the save file was created and got properly deserialized; have to recreate its chain
         {
            Debug.Log("It has this: " + errand.Get().GetType().ToString());
         }
      }

      private bool TrySetOwnerErrand() {
         Type errandType = Utils.ChainedErrandToErrandType(this);
         if(!errandType.IsSubclassOf(typeof(Workable)))
            throw new Exception(Main.debugPrefix + $"Failed to get the owner errand type for ChainedErrand {this.GetType()}");

         Workable ownerErrand = (Workable)this.gameObject.GetComponent(errandType);
         if(ownerErrand != null)
         {
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