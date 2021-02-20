﻿
## MineralExhaustionNotified v0.5

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