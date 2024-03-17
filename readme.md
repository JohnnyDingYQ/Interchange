# Description and Motivation
Interchange is a indie game in which you **build highways and interchanges**! This game was inspired by **Cities: Skylines** and **Mini Motorways**. I love building highways in Cities: Skylines, but I am less intrigued by the city planning and simulation aspect of the game. The economy system of Cities: Skylines 2 is broken and bugged anyway (as of March 6, 2024). As a result, I decided to make an indie game that does one thing and one thing well: building highways! The game will generate incrementing traffic demand (similar to Mini Motorways), and the players' job is to build highways to satisfy the **increasing demand**. Players will build on the terrain of **famous locations** such as NYC and Shanghai. It will be possible to open Google Maps along with the game and **emulate the real world highway network**. In my opinion, this is why the game idea is exciting.

# Programming Architecture
This project employs a custom version of Model View Presenter (MVP) as the primary architecture. One of the main outcome is the sepration of gameplay logic (also known as buisness logic) from user interface (Unity Game Engine)

## Model
Model contains classes which defines how data is structured, organized, manipulated, and accessed in the game. Field and properties define structure and organization, while methods define manipulation and access.

## Presenter
Presenter defines gameplay logic, operating model class objects and calling their methods to implement a meaningful game.

It is worth noting that the Model and Presenter layer has minimal dependency on Unity. The code will still work without Unity as long as one provides alternative implementations for Bezier Curves, Splines, and Vector3. This follows the **sepration of concern** principle.

## View
Unity specific code that handles input (from player) and output (rendering) of the game. Classes that implements Monobehavior lives exclusively in View.

View provides implementation of **boundary interfaces** to provide input and output to the Presenter layer.

# Roadmap and Milestones
## Highway Building (WIP)

Functionality  | Status    | Commit | Date
-------------  | ------------- | ------ | -----
Highway with different lanes |✅| [d7d0fe8](https://github.com/JohnnyDingYQ/Interchange/commit/d7d0fe8b6d440a1d1ee73389656a59b13a8f3878) | Feb 08 2024
Connecting one lane highway to another highway | ✅ | [5fdd129](https://github.com/JohnnyDingYQ/Interchange/commit/5fdd129beea3df90c4ff2bc53a3f2ccbeae1736c) | Feb 18 2024
Connecting multi lane highway to another highway <br> with more or equal lanes | ✅ | [9f82bd7](https://github.com/JohnnyDingYQ/Interchange/commit/9f82bd7e4b01982f32a4749ebb24edd74ea92d4c) | Feb 25 2024
Enfore smooth highway connections | ✅ | [de89b6d](https://github.com/JohnnyDingYQ/Interchange/commit/de89b6ddcc8bebc2b23774c62e6e19739ea8e9cf) | Feb 26 2024
Highway lane expansion | ✅ | [bbbcf2c](https://github.com/JohnnyDingYQ/Interchange/commit/bbbcf2cf8037e79f3738ed81f23d5bb28946dd2f) | Mar 03 2024
Max and Min highway lengths | ✅ |  [7c8d073](https://github.com/JohnnyDingYQ/Interchange/commit/7c8d07310d0ab34f225301aca09070ebeb26a834) | Mar 05 2024
Delete highway | ✅ | [5dac371](https://github.com/JohnnyDingYQ/Interchange/commit/5dac3718fc7f118fbd22a012a0619ccee86a23dc) | March 14 2024
Highway division | ✅ | [b5a2e0b](https://github.com/JohnnyDingYQ/Interchange/commit/b5a2e0be83f40afbebcd8516bf08bb117eab8c68) | March 15 2024
Highway replacement | WIP |


## Save System & Game State Recovery (WIP)

Functionality  | Status    | Commit | Date
-------------  | ------------- | ------ | -----
Saving and loading highways |✅| [8fbc21e](https://github.com/JohnnyDingYQ/Interchange/commit/8fbc21ecaf96daacaffb463fc29b4b7a56e030ce) | Feb 23 2024

## Vehicle pathfinding (To do)

Functionality  | Status    | Commit | Date
-------------  | ------------- | ------ | -----
Paths for vehicle | WIP |

## Highway Mesh (To do)
## Sample level (To do)
## Milestone: Prototype
## Milestone: Demo
## Milestone: Release