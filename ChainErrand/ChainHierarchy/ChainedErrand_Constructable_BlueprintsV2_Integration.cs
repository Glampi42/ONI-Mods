using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChainErrand.ChainHierarchy
{
    class ChainedErrand_Constructable_BlueprintsV2_Integration
	{

		///BlueprintsV2 integration
		public static JObject Blueprints_GetData(GameObject arg)
		{
			if (arg.TryGetComponent<ChainedErrand_Constructable>(out var component))
			{
				return new JObject()
					{
						{ "serializedChainID", component.parentLink?.parentChain.chainID ?? -1},
						{ "serializedLinkNumber", component.parentLink?.linkNumber ?? -1},
						{ "serializedChainColor", (component.parentLink?.parentChain?.chainColor ?? Color.clear).ToHexString()},
					};
			}
			return null;
		}
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

				var t3 = jObject.GetValue("serializedChainColor");
				if (t3 == null)
					return;

				SerializationUtils.ReconstructChain(t1.Value<int>(), t2.Value<int>(), targetComponent, Util.ColorFromHex(t3.Value<string>()));
			}
		}
	}
}
