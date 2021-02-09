# DSP Plugins by AleksLT

## How to Install :

First Install Bepinex in your game
folder : https://bepinex.github.io/bepinex_docs/master/articles/user_guide/installation/index.html?tabs=tabid-win

Then Download the latest DLL of the mod : https://github.com/alekslt/DSPPlugins_ALT/releases

And add it to a new subfolder in bepinex plugins folder under the steam game folder : %steamapps\common%\Dyson Sphere Program\BepInEx\plugins\DSPPlugins_ALT

Launch the game and you should be all set !

After launching one time there will be a config file you can edit in %steamapps\common%\Dyson Sphere Program\BepInEx\net.toppe.bepinex.dsp.veinexhaustion.cfg

## How to use the Mineral Exhaustion Notifier mod ?

The plugin will periodically check the miners for an alarm (Low Power, Low Production Yield), and/or if they remaining mineral they can mine is low or none.

If a miner is detected with this condition a first time notification window will pop up, but afterwards only available through the toggle button (M) on lower right,
or by pressing the hot key.

If you have cleared out all faulty miners by default this will allow a popup notification to occur again.

#### Quick Key map (configurable) : 

* I : Toggle Mineral Vein Information Window

## Screenshots

![Notification Box](https://raw.githubusercontent.com/alekslt/DSPPlugins_ALT/master/MineralExhaustionNotifier/Screenshots/InfoWindow.PNG)

![Full screen example image](https://raw.githubusercontent.com/alekslt/DSPPlugins_ALT/master/MineralExhaustionNotifier/Screenshots/FullGame.PNG)

## Changes in last release

### MineralExhaustionNotified v0.4

* v0.4.1 - Fixes a nullpointer exception when starting a new game.

* Fixed the issue where vein type was none for completely exhausted veins.
* Added textures for the resources/veins and for the menu button.
* Calculate estimated time until exhaustion for the veins.


## Verified working with game versions

* Dyson Sphere Project 0.6.16.5759
* Dyson Sphere Project 0.6.15.5706
* Dyson Sphere Project 0.6.15.5686

## Acknowledgements

* Readme structure heavily inspired/copied from https://raw.githubusercontent.com/Touhma/DSP_Plugins
* Thanks to DSP Modding Discord
* UnityRunTimeEditor for the stop mouse events from propagating through window
* xiaoye97 for their LDBTool source that allowed me to understand enough GUI in the game to make my own
* ragzilla for DSP_MinerOverride giving me a nice place to look for mining-related classes
* Forked for poking me to create the Mineral Exhaustion notification plugin.

