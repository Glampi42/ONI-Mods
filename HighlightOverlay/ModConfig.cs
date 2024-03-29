﻿using Newtonsoft.Json;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay {
   [Serializable]
   [RestartRequired]
   [ConfigFile(SharedConfigLocation: true)]
   public class ModConfig : SingletonOptions<ModConfig> {

      [Option("STRINGS.GLAMPISTRINGS.MODCONFIG.HIGHLIGHTBURIEDGEYSERS", "STRINGS.GLAMPISTRINGS.MODCONFIG.HIGHLIGHTBURIEDGEYSERS_TOOLTIP")]
      [JsonProperty]
      public bool HighlightBuriedGeysers { get; set; }

      [Option("STRINGS.GLAMPISTRINGS.MODCONFIG.ALLOWNOTPAUSED", "STRINGS.GLAMPISTRINGS.MODCONFIG.ALLOWNOTPAUSED_TOOLTIP")]
      [JsonProperty]
      public bool AllowNotPaused { get; set; }

      public ModConfig() {
         HighlightBuriedGeysers = false;
         AllowNotPaused = false;
      }
   }
}