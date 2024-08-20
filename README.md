# Discription
This is CakeBuild, created when using the template for making a mod for vintage story. It is designed by defalt to be used in a manner with a single Project within Visual Studio 2022. However, due to the nature of the project, modifications where needed to the code, in order to allow multiple mods to be built and released at once, as the individual mods are now included in one solution file.

# Use
To use the CakeBuild, when the project is open, you should see a option for running CakeBuild prodject. When ran, this will exicute a custom console application that will build the selected projects and export all neccasary Assets and compiled code to a temporry folder. Once all of the files are gathered for a projected, it is zipped up into a zip folder and the tempory fold deleted. 

To Select a specific project for release, add/remove the specific prodject name to the array on Line 27. the program will itterate through the list of names and build and package, all of the selected releases.

# Credits
- CakeBuild is build when using the [Vintage Story Mod Template](https://github.com/anegostudios/vsmodtemplate), Created by [Th3Dilli](https://github.com/Th3Dilli) a member of [Angle Stuidios](https://github.com/anegostudios).

- Modified by [Saruan The White](https://github.com/JonathanArendt) a member of this project inorder to better allow the functionalies needed by the orginization.
- Bug Fixes and Refactoring by [Clae Oczachowski](https://github.com/claeoz)
