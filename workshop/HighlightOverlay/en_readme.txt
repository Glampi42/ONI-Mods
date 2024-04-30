A QoL mod for visualizing things that in some way relate to the selected object.

How To Use

1. Select an object(any object on the map)
2. Turn on the Highlight Overlay
(2.5.) You can close the selected object's details screen so that it doesn't overlap with the overlay menu
3. Configure the things that should be highlighted (and how to highlight them) in the overlay menu

As said above, this mod lets you find and visualize things that relate to the selected object. The relation types include (but are not limited to):

• copies of this object (all its occurences on the map)
• consumers of this object (if it can be consumed)
• producers of this object
• consumable materials of this object (if this object can consume things)
• produce of this object

You may also apply filters to only highlight things of some specific kind. For example, if you want to highlight all coal debris that is laying on the floor, you can select coal (either debris or the natural tile) and highlight its copies. Then you can configure the Highlight Filters to only highlight items that lay on the floor - and voila!

To prevent the game from lagging, this mod only scans the whole map once and then highlights all the appropriate things. This means that the highlighting doesn't update if gas moves around the room, steam condenses to water, a new building gets built etc. That's why per default the overlay is only functionable while the game is paused - so that nothing on the map changes. You can disable this function in mod's config, but be aware that the information this mod shows will get incorrect over time as you let the simulation run for some time.

Tips

Here is some more specific information for people that really want to squeeze all the functionality out of this mod.

All the highlight options starting with "consider" (consider aggregate state etc.) apply also if the selected object is of a kind that doesn't have this consider-option. For example, if you select any element and check the "consider aggregate state" option and then select a building (let's say a glass forge) and highlight its produce, then the elements it produces will only be highlighted if they are in the same aggregate state as the ones that exit this building (in case with glass forge - only liquid glass will be highlighted).

The "Keep Highlighting" option also preserves the highlighting objects if you close the overlay. If you unpause the game however, it will still erase all the information (unless you enabled the overlay usage while the game is not paused in mod config).

You can select objects that are located in a storage/building and highlight things that relate to them. To do this, select the storage bin (or whatever other building) and scroll down its details menu until you see the "Contents" tab. There you can click on any entry and select that stored item!

If you want to highlight some very specific set of items, you can use a storage bin to do so. Select an empty storage bin and set its storage filters to whatever things you want to highlight. Then simply highlight its "consumables".

Special Thanks

...to Peter Han for his PLib that made the creation of the overlay menu easier!

...to all modders at the ONI's discord community for answering all my questions!

Source code is located on my GitHub[https://github.com/Glampi42/ONI-Mods/tree/main/HighlightOverlay].
You can leave your feedback, requests or bugs you found down in the comments or on GitHub by creating an issue.