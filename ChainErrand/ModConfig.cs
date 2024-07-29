using Newtonsoft.Json;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainErrand {
   [Serializable]
   [RestartRequired]
   [ConfigFile(SharedConfigLocation: true)]
   public class ModConfig : SingletonOptions<ModConfig> {

      [Option("STRINGS.GLAMPISTRINGS.MODCONFIG.DISABLEUIHELP", "STRINGS.GLAMPISTRINGS.MODCONFIG.DISABLEUIHELP_TOOLTIP")]
      [JsonProperty]
      public bool DisableUIHelp { get; set; }

      public ModConfig() {
         DisableUIHelp = false;
      }
   }
}
