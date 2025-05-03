# RTS IA

RTS project developed in Unity with the objective of programming an artificial intelligence.

## Lancement

Add the project to __Unity Hub__ and launch it with __Unity 2020.3.5f1__.
A Windows build is available in the `Build\Windows\` folder.

## Perception

The Perception system is a **Behaviour Tree** a looping sequence of goals.  
The possible goals are as follows:
* Conquer ;
* Construct Light UT ;
* Construct Heavy UT ;
* Defend ;
* Destroy ;
* Explore ;
* Secure ;
* Repair.  
  
This continuously running sequence is used to evaluate the feasibility of each goal in order to determine which ones are achievable, and to identify the __highest-priority goal__ based on the game state and the AI's perception.
To assess the goals, the system uses various __Fog of War__ data available in the AIController. Once selected, the chosen goal is then sent to the AIController.

## Fog of War

The Fog of War system provides the Unit Controller and by extension, both the Player Controller and the AI Controller—with a list of visible target buildings, enemy units, and enemy factories. Here's how it works: 
* The **Fog Of War** contains __two grids__ : 
    * One that represents what __is currently visible__ 
    * Another that represents what has been __previously seen__ 

Each entity in the game is assigned a __VisionEntity__ script, which defines its vision range within the world. This range is converted into visibility data on the grid.
Each cell of the grid contains bitflags indicating whether the red team, the blue team, or both can see that specific cell. 

For example, to add a factory to the list of visible factories, we convert its world position to the corresponding grid cell, then check the cell’s value using a bitwise operation to determine if it is currently visible to the specified team in the __“currently seen”__ grid:

```cs
VisionSystem.IsVisible(1 << (int)Team, factory.position.xz)
VisionSystem.WasVisible(1 << (int)Team, factory.position.xz)
``` 

Enemy units that are __not currently visible__ (IsVisible == false) are __hidden__, even if they were previously seen.
However, __buildings remain visible__ once they have been spotted at least once.
If a unit captures a __neutral building__, the player will only be aware of it when the building comes back into view.
The Fog of War system is fully operational.

## AIController  

It allows the creation of __buildings and units__ through inheritance from the __UnitController__.
We have added __Perception__ to this controller in order to retrieve goals and send them to the various planners, namely the __Strategical__ and __Tactical__ planners, to execute different plans.
It also maintains a list of __squads__.

## Planner

A __planner__ is used to find a list of actions that form a plan, allowing the system to move from an __initial state to a final state__ through a multi-pass search algorithm.

__State__: A sequence of values that represents the current status or configuration of the world.
__Action__: Contains both a __post-condition__ and a __pre-condition__. In other words, it defines the state before and after the action is executed, as well as the effect of this action on the world.

## Strategical Planner

This is a __Goal-Oriented Action Planning (GOAP) Planner__ with states defined by __6 integers__, forming plans in a single pass.
The planner receives a Goal and generates a __macro-level action plan__ to achieve the specified goal.

The use of 6 integers is because the world is defined as follows:
* Ressources; 
* FactoryLight; 
* FactoryHeavy; 
* IdleUnit; 
* IdleSquad.

## UpdateSquad

A single pass is used because the planner forms a plan by traversing the list of possible actions just once. (In simple terms, the planner creates a shopping list of actions to reach the final state).

The possible actions are:
* CreateLightFactory;  
* CreateHeavyFactory;  
* CreateLightUT; 
* CreateHeavyUT; 
* OrganizeArmy;  
* UpdateSquad.  

#### __post/pre condition example:__
* OrganiseArmy pre condition: currentnIdleSquad < finalNbIdleSquad;
* OrganiseArmy post condition: currentIdleSquad++;  

## Target 

A structure used for different goals, it contains a __Location__ (a place to reach), a __list of points__ to secure around a building, an __entity__ to destroy and a __building__ to conquer
* For __Conquer__, the AI will search for the __nearest discovered target building__ and capture it.
* For __Defend__, the AI will check all of its __factories__ to see if any are __under attack__, and if so, it will go and defend them.
* For __Destroy__, the AI will select one of the __enemy factories__ it has previously seen and make it the target to attack.
* For the __Explore__ goal, the AI is tasked with discovering an area that hasn’t been seen by its team. It will pick a __random point on the map__, check if it’s reachable and hasn’t been seen yet. If not, it picks another point until both conditions are met (this operation is done up to 5 times maximum).
* For __Flee__, the AI selects one of its __factories__ and retreats its troops there to escape.
* For __Secure__, the AI will select one of the __target buildings__ it has captured, create several __waypoints__, and patrol the area to secure it.
* For __Repair__, the AI will check if any of its __factories or units__ have lost health and will repair them accordingly.

## Squad

It allows for managing different formations such as:
* Unstructured;
* Line; 
* Column;
* Square;
* Triangle.

Each squad has its own formation. If it is deselected and then re-selected, it will retain its formation. When the Posture of the squad is modified, the posture of each unit within the squad is also updated accordingly.

## Tactical Planner 

The Tactical part uses an __enhanced planner__ named __SAS+ -PUS (Bäckström & Nebel, 1995)__. 
In brief, __SAS+__ stands for __Simplified Action Structures__, with the "+ - PUS" representing the addition of each condition to this pattern: 
* "P" for __Post unique__, 
* "U" for __Unary__, and 
* "S" for __Single-Valued__. 
By incorporating these conditions, the __algorithmic complexity__ for plan generation can be achieved at __O(N)__ with N the number of contexts for a state definition.

### World state defined by 7 boolean values:
* IsAtLocation;
* TargetIsCaptured;
* TargetIsDead;
* TimerIsFinish;
* SquadIsFull;
* TargetIsRepaired.

### Available actions: 
* Attack;
* Goto;
* Capture;
* Idle;
* Repair;
* SearchRecruit;
* CarryOn.

### Tactical goals available:
* Conquer;
* Defend;
* Destroy;
* Explore;
* Flee;
* Repair;
* Secure.

### Post/PréCondition:
{ 1, 0, 0, -1, -1, -1 } / Capture / { 1, 1, 0, -1, -1, -1 }  

The key feature of the __Simplified Action Structure + Plus Post-Uniqueness Unariness Single-Valuedness__ allows for a __more optimized search algorithm__ because:
A value in the state can only be changed by a single action.
Each action can only change one value at a time.

Additionally, the presence of the __meta-action CarryOn__ enables the creation of __infinite plans__, which helps __avoid re-running__ the planner for plans that would be identical.

## Unit
The Unit uses a __Finite State Machine (FSM)__, which allows it to manage different __stances__ and their transitions. 
* Passive;
* Agressive;
* Repair; 

The "__Passive__" stance is the initial posture. The unit must target __enemies to attack__ or allies to repair.
The "__Aggressive__" stance allows the unit to directly __attack the nearest enemy__ within its range.
The "__Repair__" stance (available only to __Troopers__) enables the unit to repair the nearest allied unit within its range.

When a task (such as Attack, Capture, or Repair) is assigned to the unit, it will move toward the objective. Once within range, it will execute the task.

## UI

When a unit is selected, its __posture__ can be chosen in the bottom-left corner of the screen. Similarly, when a squad is selected, there is a menu to choose its __formation__.

A __minimap__ is located in the bottom-right corner and displays the __objectives__.

At the top-right, the __x1 button__ sets the game to its initial speed, while the __x2 button__ doubles the game speed.

A __menu__ is available by clicking on "__Escape__", which pauses the game. The buttons in this menu allow the game to be __resumed, restarted, or quit__.

At the top-center, the __number of manufacturing points__ and the __number of captured target buildings__ are displayed.

## Todo to enhance the project

* Add reactive postures (e.g., Facing, Pursuit, etc.)

* Implement management of meta-tasks like CarryOn in the Tactical Planner.

* Add search and identification of entities within the TargetStruct in the planner to facilitate architecture.

* Add movement via the minimap.

* Implement waypoint placement for the player.

* Upgrade the Tactical Planner to automatically consider waypoints in its algorithm (avoid adding them via an if statement to improve code readability).

* Real-time management of formations, order units based on the closest position within the formation.

* Ensure units maintain their formation during movement.

## Images
![Alt text](Images\Example.png "Example") 
![Alt text](Images\Example2.png "Example") 
![Alt text](Images\UI.png "Example") 
![Alt text](Images\UI2.png "Example") 