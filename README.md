# mkdd-patch
Basic mod manager/installer for Mario Kart: Double Dash

## Overview
The project is currently divided by 2 seperate applications. One is a commandline application, the other, a GUI application.
The GUI application is intended for use by most users.

## mkdd-patch-gui
This is the application that allows you to manage the load order of the installed mods, the path configurations, and save & patch the changes to the game.
Upon starting the program for the first time you'll be required to enter some paths.
Fortunately for most users you only have to enter the path to the "files" directory of your extracted Double Dash image through Dolphin, 
in which case the other paths will be automatically set for you.

After the initial configuration you'll be greeted with a grid that displays information about the installed mods.
The mods are loaded bottom to top by default, meaning that the mod at the top will have the most priority.
Using the arrow keys on the right you can change the load order of mods up and down.

When you press the save button, the contents of each mod will be processed and merged accordingly and the contents will be output to the output directory.

## Creating mods
Mods can be created by creating a directory inside the mods directory. 
In that directory you'll create a folder named 'files', which will contain the replacement game files.
The files placed in there will automatically replace the existing game's files.
Next up in the mod folder should be a file named mod.xml that is formatted like so:
```
<mod>
  <title>Luigi Circuit (Reversed)</title>
  <description>Luigi Circuit is now in reverse!</description>
  <version>1.00</version>
  <author>TGE</author>
  <container path="Course/Luigi" merge="false"/>
</mod>
```
You'll see some basic information being defined about the mod. Interesting to note is the <container> tag, 
which specifies that for this container it mustn't try to merge it with the original files. This is useful for mods that don't rely on existing files.
### Containers
The patcher supports automatically creating containers (at the time of writing: .ARC, .AW) based on a folder with the same name. 
Depending on the configuration, the patcher will decide to merge it with the original files or rebuild the container without them altogether.

To give you a practical example of how this looks in practice, here's a sample mod: 
https://cdn.discordapp.com/attachments/614953013781725214/674046341231542299/Magikarp.7z
