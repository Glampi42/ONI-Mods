using Newtonsoft.Json;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier {
   [Serializable]
   [RestartRequired]
   [ConfigFile(SharedConfigLocation: true)]
   public class ModConfig : SingletonOptions<ModConfig> {

      [Option("STRINGS.GLAMPISTRINGS.MODCONFIG.PERSISTENTNOTIFICATIONS", "STRINGS.GLAMPISTRINGS.MODCONFIG.PERSISTENTNOTIFICATIONS_TOOLTIP")]
      [JsonProperty]
      public bool PersistentNotifications { get; set; }

      public ModConfig() {
         PersistentNotifications = false;
      }
   }
}
