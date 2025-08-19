## Tool for developers
Dun Gen is a procedural map generation tool with the intention to give other developers the ability to easily add randomly generated dungeons into their game.
This is planned to be released on the Unity asset store but is fairly early in development.

Dun gen uses a map generation algorithm I built from scratch to generate layouts using user customized settings.
It then uses that map to spawn 3D dungeons using customizable tile sets for different types of games.
I am still working out what features I will include in the initial release but the goal is to support a variety of game styles
and to include 2-3 prebuilt tile sets as well as allowing developers to build their own.

**Created this repo for portfolio purposes, main development is happening in a seperate private repo**

## A quick example
First a 2D map is generated based on user defined settings
<img width="1010" height="564" alt="DunGen3" src="https://github.com/user-attachments/assets/6738922b-f2dd-49a5-82d5-c0b5625da5ed" />

A 3D dungeon is spawned in matching the layout of the generated map
<img width="875" height="449" alt="DunGen3-3d" src="https://github.com/user-attachments/assets/17cf0797-9193-42d6-abc0-16886c50cefb" />

A player object is then spawned in the dungeon allowing exploration
<img width="1007" height="567" alt="DunGen3-FP" src="https://github.com/user-attachments/assets/5f5e4134-f252-4d26-8a53-096fe444a39b" />

## Description of the map generation algorithm
To give a quick summary of the map generation algorithm I will describe it roughly in steps.
1. Pick a number of tiles on the grid, making sure they are seperated by at least one tile in between them, to act as our primary rooms.
2. Iterate through each primary room, everytime we find an isolated room we construct a path to the nearest room using an A* algorithm.
   At this stage each room will be connected to at least one other room, but a path likely does not exist from any given room to any other given room.

3. Scan through the rooms to construct graphs representing subsets of rooms connected together by a network of paths.
4. Iterate through each graph, connecting it to its nearest graph similar to how we connected rooms together in step 2.
   Each time we connect two graphs together we merge them into a single graph.
5. Repeat step 4 until all graphs are merged together. This will mean all our primary rooms are connected together in a single network.

6. At this point, to add variation we create a bunch of offshoot paths called branches that have different properties based on the branch type.
7. Choose a tile to act as the dungeon entrance and another tile for the exit.

I have ideas for how to add more variation to the dungeons but for now I will flesh out the rest of the features of the tool

<img width="524" height="293" alt="DunGen1" src="https://github.com/user-attachments/assets/bf676333-4b34-4f29-a095-0c5e62f61198" />
<img width="524" height="293" alt="DunGen2" src="https://github.com/user-attachments/assets/2c863059-cfc4-40da-8112-f2f47d42879a" />
