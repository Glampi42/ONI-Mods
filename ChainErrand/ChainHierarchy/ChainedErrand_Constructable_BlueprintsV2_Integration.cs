using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ChainErrand.Strings.MYSTRINGS.UI.TOOLS;

namespace ChainErrand.ChainHierarchy
{
	class ChainedErrand_Constructable_BlueprintsV2_Integration
	{
		/// <summary>
		/// Gets data from a chained_errand_constructable component to be used in blueprints if it exists on the gameobject.
		/// </summary>
		/// <param name="arg">building gameobject that is potentially a planned building</param>
		/// <returns>JObject that contains the data that is stored inside the blueprint</returns>
		public static JObject Blueprints_GetData(GameObject arg)
		{
			if (arg.TryGetComponent<ChainedErrand_Constructable>(out var component))
			{
				return new JObject()
					{
						{ "serializedChainID", component.parentLink?.parentChain?.chainID ?? -1},
						{ "serializedLinkNumber", component.parentLink?.linkNumber ?? -1}
					};
			}
			return null;
		}

		/// <summary>
		/// Sets the data for a chained constructable from a blueprint.
		/// </summary>
		/// <param name="building">building gameobject that is potentially a building under construction with a chainedErrand_constructable component</param>
		/// <param name="jObject">the data that was stored inside the blueprint</param>
		public static void Blueprints_SetData(GameObject building, JObject jObject)
		{
			if (jObject == null)
				return;

			if (building.TryGetComponent<ChainedErrand_Constructable>(out var targetComponent))
			{
				var t1 = jObject.GetValue("serializedChainID");
				if (t1 == null)
					return;

				var t2 = jObject.GetValue("serializedLinkNumber");
				if (t2 == null)
					return;

				AccumulateBlueprintChainData(t1.Value<int>(), t2.Value<int>(), targetComponent);
			}
		}

		/// <summary>
		/// caches the chained constructables that were placed by blueprints to create new chains from them
		/// </summary>
		static Dictionary<int, Dictionary<ChainedErrand_Constructable, int>> AccumulatedChains = new();

		/// <summary>
		/// stores the chained constructable data in a cache to be used later for chain creation.
		/// </summary>
		/// <param name="chainId">chainId of the chain from blueprint creation, used to group the errands</param>
		/// <param name="linkNumber">link number in the chain</param>
		/// <param name="chainedErrand">the errand itself</param>
		public static void AccumulateBlueprintChainData(int chainId, int linkNumber, ChainedErrand_Constructable chainedErrand)
		{
			if (chainId == -1 || linkNumber == -1 || chainedErrand == null || chainedErrand.IsNullOrDestroyed())
				return;

			if (!AccumulatedChains.TryGetValue(chainId, out var chainedConstructablesCache))
			{
				chainedConstructablesCache = new();
				AccumulatedChains.Add(chainId, chainedConstructablesCache);
			}
			chainedConstructablesCache[chainedErrand] = linkNumber;

			///only schedule chain creation if there isnt already one pending
			if (ConstructionPending)
				return;

			ConstructionPending = true;
			///schedule the chain creation for the next frame since by then all other buildings from the blueprint should have had their data cached
			chainedErrand.StartCoroutine(DelayedChainCreation());
		}
		private static bool ConstructionPending = false;

		static IEnumerator DelayedChainCreation()
		{
			yield return null; // wait for the next frame
			ConstructAccumulatedBlueprintChains();
		}

		public static void ConstructAccumulatedBlueprintChains()
		{
			foreach (var cachedChain in AccumulatedChains.Keys)
			{
				var chainData = AccumulatedChains[cachedChain];
				if (chainData.Count == 0)
					continue;

				// creating a new chain:
				var errands = new Dictionary<int, Dictionary<GameObject, HashSet<Workable>>>();

				foreach(var cachedConstructable in chainData)
				{
					int linkNumber = cachedConstructable.Value;
					var chainedErrand = cachedConstructable.Key;

					if (chainedErrand == null || chainedErrand.IsNullOrDestroyed() || linkNumber == -1)
						continue;

               Dictionary<GameObject, HashSet<Workable>> linkErrands;
					if(!errands.TryGetValue(linkNumber, out linkErrands))
					{
						linkErrands = new();
						errands.Add(linkNumber, linkErrands);
					}

					linkErrands.Add(chainedErrand.gameObject, new() { chainedErrand.Errand });

					// grabbing potential diggables:
					if (chainedErrand.Errand is Constructable constructable)
					{
						constructable.building.RunOnArea(cell =>
						{
							Diggable diggable = Diggable.GetDiggable(cell);

							if (diggable.IsNullOrDestroyed() || !diggable.enabled)
								return;

							linkErrands.Add(diggable.gameObject, new() { diggable });
						});
					}
				}

            Chain chain = ChainsContainer.CreateNewChain();
            foreach(int linkNumber in errands.Keys)
				{
					chain.CreateOrExpandLink(linkNumber, true, errands[linkNumber], true);
				}

				SerializationUtils.CleanupEmptyDeserializedChains();// in case some links are missing for whatever reason
			}
			ConstructionPending = false;
			AccumulatedChains.Clear();
		}
	}
}
