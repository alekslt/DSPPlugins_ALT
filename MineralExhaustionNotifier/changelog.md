## MineralExhaustionNotifier v0.5.4

* Show custom planet names. Contribution from Enrique Ramirez
* Fix for new big update of DSP. 0.9.24.11286. Contribution from dgschrei
* Added auto collapsed mode for miners and logistic stations (helps a lot performance wise with large amount of miners/stations)
* Internal code restructuring.

### Commit log from 'v0.5.2' to 'v0.5.4' (most recent changes are listed on top):

a20fd2f Move show/hide GUI behind member functions and not a static toggle to allow me to run initialization code on switches.
b638c5a Turn auto update on by default as ICBA to optimize the initialization path for the sources.
8a26ab5 Update target framework to modern/netstandard and set font size explicitly as new defaulty unity one is too big. Add code for script engine hot reload.
b5fd67e WIP for MineralExhaustionNotifier 5.2.3. fix for new release of DSP
51f404f Merge pull request #8 from dgschrei/masterchange uiGame.dysonmap to uiGame.dysonEditor
8c996aa Merge pull request #7 from enriquein/masterShow custom planet names in Logistic Stations view.
3e38437 change uiGame.dysonmap to uiGame.dysonEditor
8a4a1bf Show custom planet names in Logistic Stations view.
89fb1c9 MineralExhaustionNotifier: Fixed style for open/close group symbol. Added showing text on the filter.
c5d2989 Add additional grouped levels for miners and logistic stations.
e3cc38c Introduce multiple default collapsed state levels. 1-3.
4c54280 Fix Auto Update Toggle (Earlier always updated since it was bound to the collection)
6fcd561 Fix the filter for collectors
fd757a6 Added auto collapsed mode for miners and logistic stations (helps a lot performance wise with large amount of miners/stations)


## MineralExhaustionNotifier v0.5.2

* As some had grown used to the smaller window size with high resolution/pixel densities the scaling code is reworked to base itself on the games UI Layout Reference Height adjustment.
* If you are not happy with the size that the UI Layout Reference Height adjustment gives you there are config options that allow you to override the size. (Larger value = smaller window)
* In game window scale adjustment through - 1 + buttons on the window title bar.
* Fixed sorting for Logistic Stations - Resource. Now sorts on Resource Name, not the internal id. :)

### Commit log from 'v0.5.1' to 'v0.5.2' (most recent changes are listed on top):

7381027 Prepare for release MineralExhaustionNotified v0.5.2. Updated version, readme, changelog, screenshots and thunderstore icon/logo.
7c20594 Added two config variables for adjusting the UI Layout Reference Height behaviour for the UI. Default is following the games reference height, but can also override. Also added runtime adjustable scale for the window.
66f175b MinerNotificationUI - Order by item name, not item id for logistic stations - resource view.


## MineralExhaustionNotifier v0.5.1

* User customizable button placement
* Scaling code to give a consistent UI size independent of resolusion. (Thanks to yushiro for PR)
* Disabled initial pop-up, if alarm situation, for now as I personally didn't find this feature useful. Will bring it back after user customizable triggers are implemented.

### Commit log from 'v0.5.0' to 'v0.5.1' (most recent changes are listed on top):

Commit log from 'MineralExhaustionNotifier-v0.5' to 'HEAD' (most recent changes are listed on top):
2dd18eb	Updating version, readme, manifest to prepare for minernotificationui release 0.5.1.
a34580f	Popups are a bad idea. Work on user settable alarm triggers in the future instead.
c12efbc	Config configurable placement of menu button. Fixes #2.
8b92039	Use ScaledScreenWidth to place menu button. Also shrunk menu button, and replaced its background texture.
4c155d1	Modified autoscale code for #1 to support various aspect ratios other than 1920x1080. (Mine is 5120x1440).
ba6b165	Merge pull request #3 from yushiro/masterFixes issue #1. Auto Scale MineralExhaustionNotifier UI based based on design size of 1080.
91ba46d	bugfix #1base on 1920x1080 GUI and this will auto scale GUI at game resolution 3840x2160


## MineralExhaustionNotifier v0.5

* Added information on logistic stations and their items as well
* Filtering options. Filters are logically AND between the filters, and OR for the filter-local. Meaning you can do queries like: Stellar Stations AND Item % below 50%, AND Supply Remote.
* Show information from the data source ( Vein Miners or Logistic Stations) broken down into different groups. Planet style as it was, or Resource based to get a better understanding of everything related to titanium.
* Keybinding is now changed! You need to press an ALT-key before the user bindable key (default I)

### Commit log from 'v0.4.1' to 'v0.5' (most recent changes are listed on top):

25ce0a7 Bumped version to 0.5 for MineralExhaustionNotifier. Ready for realase. Updated screenshots, readme and changelog.
5f2adcf Clearer filter text (less than sign). Show position of logistic stations. Additional filters. Now force to hold Alt-key + bound key to open.
a33f31f More MinerNotificationUI cleanup. Initial UpdateSource logic in place. Removed old stat structure from DSPStatistics.
f061b61 Tried to clean up the UI stuff. Precalc the filtered stuff on change, not every gui-cycle.
dd1f86f Added filters on the main data sources. Adjustable from a filter-line in the GUI. Added logistic stations as a new source.
e2fc22d Refactored some of the statistic methods to make it easier to introduce new sources.
97cb50d Moved Thunderstore specific files to a subfolder. Use a shared included T4 template to generate manifest.json. Moved FDQN NS for plugins into Version.cs. Created a post-build release-prep script that updates a current release-folder. Currently for me that would be alekslt-$ProjectName-v0.0 with all the files that should be in the release.
e216b34 Use a common Version.cs file for assemblyinfo, bepinex and manifest.json. Use T4 to generate manifest.json


## MineralExhaustionNotifier v0.4.1

* Fixes a nullpointer exception when starting a new game.

### Commit log from 'v0.4' to 'v0.4.1' (most recent changes are listed on top):
e18721e Fixes a NPE in onGUI check for game state. Didn't guard some variables that was not initialized under intro guide. Bumps version to 0.4.1


## MineralExhaustionNotifier v0.4

* Fixed the issue where vein type was none for completely exhausted veins.
* Added textures for the resources/veins and for the menu button.
* Calculate estimated time until exhaustion for the veins.

### Commit log from 'v0.3' to 'v0.4' (most recent changes are listed on top):

6508348 Bumped versions for release 0.4 and added new screenshots
b0a230e Added changelog.txt
50db0a9 Replaced menu button background with a round circle texture
4522d09 Get last mined resource from signstate.icon. Draw resource texture and alarm texture. Cleanup.


## MineralExhaustionNotifier v0.3

* Fixed: The notification box was retriggered too often due to a flag not being set.

### Commit log from 'v0.2' to 'v0.3' (most recent changes are listed on top):

c0fc1d0 Update README.md
7430df7 The notification box was retriggered too often due to a flag not being set. Fixed images in readme for thunder. Only copy local files to output that should be a part of the release.



## MineralExhaustionNotifier v0.2

* Hide GUI when F11 - hidden is active.
* Show GUI when in space/sail mode.
* Fixed a bug where the input capture for the notification window was too greedy, resulting in a non-controllable flying mech. Now only captures mouse events inside the window.

### Commit log from 'v0.1' to 'v0.2' (most recent changes are listed on top):

578df85 Added back root readme, fixed location for images in mineralexhaustionnotifier readme. Added logo and manifest.json for dsp.thunderstore.io
3b4cf2e Moved project to a subfolder. Made output from debug build go to normal bin/Debug, but instead have a post-build check that copies the result over if it is on my dev computer.
27db380 GUI improvements, refactoring and input event capture cleanup. See list below:* Highlight button red immediately.
        * Moved out some DSP - mapping functions to a helper class.
        * Show gui also in space, but hide it in the no gui mode.
        * Don't eat keyboard events, only mouse.
        * Close on escape key.
ceb3a7c Update README.md



## MineralExhaustionNotifier v0.1

First release. Works fine for me, but haven't been tested on other peoples computers yet. RFC.
The plugin will periodically check the miners for an alarm (Low Power, Low Production Yield), and/or if they remaining mineral they can mine is low or none.

If a miner is detected with this condition a first time notification window will pop up, but afterwards only available through the toggle button (M) on lower right,
or by pressing the hot key.

If you have cleared out all faulty miners by default this will allow a popup notification to occur again.