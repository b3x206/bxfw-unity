# bxfw-unity

## Note :
**You are currently looking at the `bxfw-legacy` branch, which is outdated. It will remain only for compatibility reasons for the games with older code bases that are very integrated with it or for the games with code bases that i have no will to update to newer versions of bxfw.**

This `bxfw-legacy` branch will only exist for bug fixes and will stay API compatible forever. But keep in mind that the legacy version is programmed in a cursed way.

---
## What is it? :
[![CodeFactor](https://www.codefactor.io/repository/github/b3x206/bxfw-unity/badge)](https://www.codefactor.io/repository/github/b3x206/bxfw-unity)

Prototyping and game development tools to make it more convenient, the old version before i broke compatibility with older games.
The code is &lt;summary/&gt; ified, basically what the code does is in the code file (which is not docs).

## How to use : 
* Clone using ```$ git clone -b bxfw-legacy https://github.com/b3x206/bxfw-unity.git``` to desired directory (somewhere inside Assets folder, it has to be inside Assets). (Or download the zip of source code and import it to the Assets folder)
* To clone into a folder with different name just add a name argument like ```$ <git command for clone> desired_folder_name```.
* This cloning only applies for the `bxfw-legacy` branch.
* When added, the BXFW will add 2 more assemblies/csproj to your unity project. You can edit it to your hearts content and fix bugs or delete the folder to get rid of it if you are not me.

## Problem / Bug? :
* Only for bugs/regressions, open an issue with the `bxfw-legacy-version` tag.
* Or try to solve and if you succeed open PR yay.
* No new features will be added but some features that may be left half way can be completed.
* This version's scripts could be updated to the newer versions of unity, but the current version should be compatible with unity `2020.x` or newer.

## Other Stuff : 
* Portions of the project have been copied from https://github.com/nothke/unity-utils and is owned by Ivan Notaroš under the terms of the MIT license. See https://github.com/nothke/unity-utils/blob/9ba0ae06674f4a6be67ddbbfcc876f72ee6eb2b1/Runtime/RTUtils.cs for the exact file copied.
* Portions of the project have been copied from https://github.com/gkjohnson/unity-dithered-transparency-shader and is owned by Garrett Johnson under the terms of the MIT license.
* These need to be attributed inside some portion of your game, whether if it be credits or not. If these files are completely removed or not included with compiled builds of your game it may not be needed, but i am not a lawyer.

## Warning :
* `bxfw-legacy` branch will remove some shaders/scripts that could be problematic to be licensed/or very deprecated stuff that don't work normally in newer versions of unity (like unity 4 old shaders, yeah)
