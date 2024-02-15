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
      TILES = 0x8,
      LIQUID_PIPES = 0x10,
      GAS_PIPES = 0x20,
      RAILS = 0x40,
      WIRES = 0x80,
      AUTOMATION = 0x100,
      BACKWALLS = 0x200,
      BUILDINGS = STANDARD_BUILDINGS | TILES | LIQUID_PIPES | GAS_PIPES | RAILS | WIRES | AUTOMATION | BACKWALLS,

      LIQUID_CONTENTS = 0x400,
      GAS_CONTENTS = 0x800,
      RAILS_CONTENTS = 0x1000,
      CONDUIT_CONTENTS = LIQUID_CONTENTS | GAS_CONTENTS | RAILS_CONTENTS,

      NATURAL_TILES = 0x2000,
      LIQUIDS = 0x4000,
      GASES = 0x8000,
      CELLS = NATURAL_TILES | LIQUIDS | GASES,

      PLANTS = 0x10000,
      CRITTERS = 0x20000,
      DUPLICANTS = 0x40000,
      ROBOTS = 0x80000,
      CREATURES = PLANTS | CRITTERS | DUPLICANTS | ROBOTS,

      GEYSERS = 0x100000,
      OTHER = 0x200000,
      MISCELLANEOUS = GEYSERS | OTHER,

      ALL = 0x400000 - 1
   }
}
