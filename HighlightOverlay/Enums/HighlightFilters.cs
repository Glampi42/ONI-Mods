using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightOverlay.Enums {
   [Flags]
   public enum HighlightFilters {
      NONE = 0x0,

      ON_GROUND = 0x1,
      STORED_ITEMS = 0x2,
      ITEMS = ON_GROUND | STORED_ITEMS,

      STANDARD_BUILDINGS = 0x4,
      LIQUID_PIPES = 0x8,
      GAS_PIPES = 0x10,
      RAILS = 0x20,
      WIRES = 0x40,
      AUTOMATION = 0x80,
      BACKWALLS = 0x100,
      BUILDINGS = STANDARD_BUILDINGS | LIQUID_PIPES | GAS_PIPES | RAILS | WIRES | AUTOMATION | BACKWALLS,

      LIQUID_CONTENTS = 0x200,
      GAS_CONTENTS = 0x400,
      RAILS_CONTENTS = 0x800,
      CONDUIT_CONTENTS = LIQUID_CONTENTS | GAS_CONTENTS | RAILS_CONTENTS,

      NATURAL_TILES = 0x1000,
      LIQUIDS = 0x2000,
      GASES = 0x4000,
      CELLS = NATURAL_TILES | LIQUIDS | GASES,

      PLANTS = 0x8000,
      CRITTERS = 0x10000,
      DUPLICANTS = 0x20000,
      ROBOTS = 0x40000,
      CREATURES = PLANTS | CRITTERS | DUPLICANTS | ROBOTS,

      GEYSERS = 0x80000,
      OTHER = 0x100000,
      MISCELLANEOUS = GEYSERS | OTHER,

      ALL = 0x200000 - 1
   }
}
