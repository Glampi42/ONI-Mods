using ChainErrand.Strings;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainHierarchy {
   [SerializationConfig(MemberSerialization.OptIn)]
   public class ChainedErrand : KMonoBehaviour {
      public static readonly Chore.Precondition ChainedErrandPrecondition = new() {
         id = nameof(ChainedErrandPrecondition),
         description = MYSTRINGS.UI.CHOREPRECONDITION.NOTFIRSTLINK,
         fn = (ref Chore.Precondition.Context context, object _) => {
            if(context.chore.TryGetCorrespondingChainedErrand(context.chore.prioritizable.gameObject, out ChainedErrand chainedErrand))
            {
               return chainedErrand.parentLink == null || chainedErrand.parentLink.linkNumber == 0;
            }

            return false;
         }
      };

      [Serialize]
      public int test;
      public Link parentLink;

      public Workable errand;
      public Chore chore;
      public GameObject chainNumberBearer;// the GameObject that the chain number will be displayed on (isn't always the GameObject that has the errand component: f.e. MoveTo errand)

      public void ConfigureChorePrecondition(Chore chore = null) {
         if(chore == null)
            chore = Utils.GetChoreFromErrand(errand);
         
         this.chore = chore;

         if(chore != null)
         {
            chore.AddPrecondition(ChainedErrandPrecondition);
            if(parentLink.linkNumber != 0)// stop dupes from doing errands that are not in the first link
               chore.Fail("Chore added to chain");
         }
      }

      public override void OnCleanUp() {
         base.OnCleanUp();

         Remove(true, false);

         parentLink = null;
         UpdateChainNumber();
      }

      public void UpdateChainNumber() {
         if(Main.chainOverlay != default)
         {
            Main.chainOverlay.UpdateChainNumber(chainNumberBearer, errand, parentLink);
         }
         test++;
      }

      public void Remove(bool tryRemoveLink, bool destroySelf = true) {
         if(tryRemoveLink)
         {
            parentLink.errands.Remove(this);
            if(parentLink != null && parentLink.errands.Count == 0)
            {
               parentLink.Remove(true);
            }
         }

         if(destroySelf)
         {
            // removing the chore precondition:
            if(chore != null)
            {
               chore.preconditions.Remove(chore.preconditions.FirstOrDefault(precondition => precondition.id == nameof(ChainedErrandPrecondition)));
            }

            UnityEngine.Object.Destroy(this);
         }
      }
   }
}
