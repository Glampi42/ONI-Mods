﻿/*
 * Copyright 2024 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using ErrandNotifier.Enums;
using ErrandNotifier.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrandNotifier {
   public sealed class NotifierToolHover : HoverTextConfiguration {
      public override void UpdateHoverElements(List<KSelectable> selected) {
         var hoverInstance = HoverTextScreen.Instance;
         var drawer = hoverInstance.BeginDrawing();

         string textToDraw = "TOOL NOT FOUND";
         if(Main.notifierTool != default)
         {
            switch(Main.notifierTool.GetToolMode())
            {
               case NotifierToolMode.CREATE_NOTIFICATION:
                  textToDraw = MYSTRINGS.UI.NOTIFIERTOOLMENU.CREATENOTIFICATION.text.ToUpper();
                  break;

               case NotifierToolMode.ADD_ERRAND:
                  textToDraw = MYSTRINGS.UI.NOTIFIERTOOLMENU.ADDERRAND_HOVER.text.ToUpper();
                  break;

               case NotifierToolMode.DELETE_NOTIFICATION:
                  textToDraw = MYSTRINGS.UI.NOTIFIERTOOLMENU.DELETENOTIFICATION.text.ToUpper();
                  break;

               case NotifierToolMode.REMOVE_ERRAND:
                  textToDraw = MYSTRINGS.UI.NOTIFIERTOOLMENU.REMOVEERRAND.text.ToUpper();
                  break;
            }
         }

         // Draw the tool title
         drawer.BeginShadowBar(false);
         drawer.DrawText(textToDraw, ToolTitleTextStyle);
         // Draw the instructions
         ActionName = global::Strings.Get("STRINGS.UI.TOOLS.DIG.TOOLACTION");
         DrawInstructions(hoverInstance, drawer);

         drawer.EndShadowBar();
         drawer.EndDrawing();
      }
   }
}
