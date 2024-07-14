using ChainErrand.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace ChainErrand {
   public class ChainToolFilters {
      public static List<Filter> allFilters = new List<Filter>();

      public static readonly Filter All = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.ALL);

      public static readonly Filter Construction = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.CONSTRUCTION);
      public static readonly Filter Dig = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.DIG);
      public static readonly Filter Sweep = new Filter(MYSTRINGS.UI.TOOLS.FILTERLAYERS.SWEEP);
      public static readonly Filter Mop = new Filter(MYSTRINGS.UI.TOOLS.FILTERLAYERS.MOP);
      public static readonly Filter EmptyPipe = new Filter(STRINGS.UI.TOOLS.EMPTY_PIPE.NAME);
      public static readonly Filter MoveTo = new Filter(STRINGS.UI.USERMENUACTIONS.PICKUPABLEMOVE.NAME);

      public static readonly Filter Standard_Buildings = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.BUILDINGS);
      public static readonly Filter Liquid_Pipes = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.LIQUIDPIPES);
      public static readonly Filter Gas_Pipes = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.GASPIPES);
      public static readonly Filter Conveyor_Rails = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.SOLIDCONDUITS);
      public static readonly Filter Wires = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.WIRES);
      public static readonly Filter Automation = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.LOGIC);
      public static readonly Filter Backwalls = new Filter(STRINGS.UI.TOOLS.FILTERLAYERS.BACKWALL);

      public sealed class Filter {
         /// <summary>
         /// The name shown in the options menu.
         /// </summary>
         public string Name { get; }

         public Filter(LocString name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            allFilters.Add(this);
         }

         /// <summary>
         /// Checks to see if the filter is applied.
         /// </summary>
         /// <returns>True if the filter is applied, false otherwise.</returns>
         public bool IsOn() {
            if(ChainToolMenu.Instance == null)
               return false;

            return ChainToolMenu.Instance.GetToggleState(this) == ToolParameterMenu.ToggleState.On;
         }
      }
   }
}
