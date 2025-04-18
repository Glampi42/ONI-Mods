using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier.Enums {
   public enum NotifierToolFilter {
      ALL,
      CONSTRUCTION,
      DIG,
      MOP,
      EMPTY_PIPE,
      MOVE_TO,

      STANDARD_BUILDINGS,
      WIRES,
      LIQUID_PIPES,
      GAS_PIPES,
      CONVEYOR_RAILS,
      AUTOMATION,
      BACKWALLS
   }

   public static class NotifierToolFilterExtensions {
      /// <summary>
      /// Checks to see if the filter is applied.
      /// </summary>
      /// <returns>True if the filter is applied, false otherwise.</returns>
      public static bool IsOn(this NotifierToolFilter filter) {
         if(NotifierToolMenu.Instance == null)
            return false;

         return NotifierToolMenu.Instance.GetToggleState(filter) == ToolParameterMenu.ToggleState.On;
      }

      /// <summary>
      /// Returns the string representation of the filter.
      /// </summary>
      /// <param name="filter">The filter</param>
      /// <returns>The string.</returns>
      public static string Name(this NotifierToolFilter filter) {
         switch(filter)
         {
            case NotifierToolFilter.ALL:
               return STRINGS.UI.TOOLS.FILTERLAYERS.ALL.NAME;
            case NotifierToolFilter.CONSTRUCTION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.CONSTRUCTION.NAME;
            case NotifierToolFilter.DIG:
               return STRINGS.UI.TOOLS.FILTERLAYERS.DIG.NAME;
            case NotifierToolFilter.MOP:
               //return MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOP.NAME;
            case NotifierToolFilter.EMPTY_PIPE:
               return STRINGS.UI.TOOLS.EMPTY_PIPE.NAME;
            case NotifierToolFilter.MOVE_TO:
               return STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME;

            case NotifierToolFilter.STANDARD_BUILDINGS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BUILDINGS.NAME;
            case NotifierToolFilter.WIRES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.WIRES.NAME;
            case NotifierToolFilter.LIQUID_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LIQUIDPIPES.NAME;
            case NotifierToolFilter.GAS_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.GASPIPES.NAME;
            case NotifierToolFilter.CONVEYOR_RAILS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.SOLIDCONDUITS.NAME;
            case NotifierToolFilter.AUTOMATION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LOGIC.NAME;
            case NotifierToolFilter.BACKWALLS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BACKWALL.NAME;

            default:
               return "MISSING.NAME.FOR.FILTER." + filter;
         }
      }

      /// <summary>
      /// Returns the tooltip that is displayed when hovering over the filter.
      /// </summary>
      /// <param name="filter">The filter</param>
      /// <returns>The tooltip.</returns>
      public static string Tooltip(this NotifierToolFilter filter) {
         switch(filter)
         {
            case NotifierToolFilter.ALL:
               return STRINGS.UI.TOOLS.FILTERLAYERS.ALL.TOOLTIP;
            case NotifierToolFilter.CONSTRUCTION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.CONSTRUCTION.TOOLTIP;
            case NotifierToolFilter.DIG:
               return STRINGS.UI.TOOLS.FILTERLAYERS.DIG.TOOLTIP;
            case NotifierToolFilter.MOP:
               //return MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOP.TOOLTIP;
            case NotifierToolFilter.EMPTY_PIPE:
               //return MYSTRINGS.UI.TOOLS.FILTERLAYERS.EMPTYPIPE.TOOLTIP;
            case NotifierToolFilter.MOVE_TO:
               //return MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOVETO.TOOLTIP;

            case NotifierToolFilter.STANDARD_BUILDINGS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BUILDINGS.TOOLTIP;
            case NotifierToolFilter.WIRES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.WIRES.TOOLTIP;
            case NotifierToolFilter.LIQUID_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LIQUIDPIPES.TOOLTIP;
            case NotifierToolFilter.GAS_PIPES:
               return STRINGS.UI.TOOLS.FILTERLAYERS.GASPIPES.TOOLTIP;
            case NotifierToolFilter.CONVEYOR_RAILS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.SOLIDCONDUITS.TOOLTIP;
            case NotifierToolFilter.AUTOMATION:
               return STRINGS.UI.TOOLS.FILTERLAYERS.LOGIC.TOOLTIP;
            case NotifierToolFilter.BACKWALLS:
               return STRINGS.UI.TOOLS.FILTERLAYERS.BACKWALL.TOOLTIP;

            default:
               return "MISSING.TOOLTIP.FOR.FILTER." + filter;
         }
      }
   }
}
