# RTS IA

Projet RTS sur Unity dans l'objectif de coder une intelligence artificielle.

## Lancement

Ajouter le projet sur Unity Hub et le lancer avec Unity2020.3.5f1.  
Une build Window se trouve dans `Build\Windows\`.

## Perception

La Perception est un **Behaviour Tree** qui contient une séquence de buts qui tourne en boucle.  
Les différents Buts possibles sont les suivants :  
* Conquer ;
* Construct Light UT ;
* Construct Heavy UT ;
* Defend ;
* Destroy ;
* Explore ;
* Secure ;
* Repair.  
  
Cette séquence, qui tourne en permanence, permet de tester la faisabilité des différents But afin de savoir lesquels sont possibles, et ainsi,  de déterminer le But le plus prioritaire en fonction de l’état de la partie et de la vision de l’IA. Pour tester les Buts, on utilise les différentes informations du Fog of War contenues dans l’AIController. Enfin, ce But est envoyé à l’AIController.  


## Fog of War

Le Fog Of War donne à l’Unit Controller, donc au Player Controller et à l’IA Controller, une liste de Target Buildings, d’unités ennemies et de factories ennemies vues. Je vais vous expliquer comment ça fonctionne. Le **Fog Of War** contient 2 grilles : une qui indique ce qu’elle voit actuellement, et une autre qui montre ce qu’elle a vu. Chaque entité a un script **VisionEntity** qui contient leur _“range de vision”_ dans le World. On convertit ceci dans la grille. Chaque cellule de la grille contient une valeur en bitflags qui indique si la team rouge ou / et bleue voit cette cellule.



Par exemple, pour ajouter une factory dans la liste de factory vu, on convertit sa position dans la grille pour obtenir la cellule correspondante et on vérifie la valeur de celle-ci avec un bitwise pour la team choisie dans la grille “voit actuellement” : 

```cs
VisionSystem.IsVisible(1 << (int)Team, factory.position.xz)
VisionSystem.WasVisible(1 << (int)Team, factory.position.xz)
```

Les unités ennemis qui ne sont pas vu (IsVisible) sont cachés, même s’ils ont été vus. Les bâtiments apparaissent dès qu’ils ont été vus. Si l’unité capture un bâtiment neutre, le joueur le saura uniquement à la vue de ce bâtiment. Le Fog of War est entièrement fonctionnel.  


## AIController

Elle permet de créer des bâtiments ainsi que des unités, par le biais de l’héritage du UnitController.  
Nous avons ajouté la *Perception* à celui-ci afin de récupérer des buts pour les envoyés aux différents planner, *Strategical* et *Tactical*, qu’elle contient pour exécuter différents plans.  
Elle contient également une liste d'escouades.  


## Planner

Un planner permet de trouver une liste d’action formant un plan qui permet d’aller d’un état initial à un état final grâce à un algorithme de recherche à multiple-passe.
__État:__ c’est une suite de valeur qui permet de savoir comment se situe le monde.
Une action: contient une post condition et une précondition, autrement dit, quelle est l'état avant et après cette action, ou quelle est l’action de cette action sur le monde aussi.


## Strategical Planner

C’est un Goal Oriented Action Planning Planner avec des états à 6 int formant des plans en 1 seule pass.  
Le planner va recevoir un Goal et en sortir un plan d’action plutôt macro pour répondre au Goal envoyé.  

Il y au donc 6 int parce que le monde se définit comme ceci: 
* Ressources; 
* FactoryLight; 
* FactoryHeavy; 
* IdleUnit; 
* IdleSquad.

## UpdateSquad

Un seul passe car il forme un plan en parcourant 1 seule fois la liste d’actions possibles, (en vulgarisant, le planner fait une liste de course dans les actions possibles pour arriver à son état final).
Les actions possibles: 
* CreateLightFactory;  
* CreateHeavyFactory;  
* CreateLightUT; 
* CreateHeavyUT; 
* OrganizeArmy;  
* UpdateSquad.  

__Exemple de post pre condition :__
* OrganiseArmy pre condition: currentnIdleSquad < finalNbIdleSquad;
* OrganiseArmy post condition: currentIdleSquad++;  

## Target 
Une structure utilisée pour les différents Buts. Elle contient une Location, qui est un lieu où se rendre. Une liste de points à sécuriser autour d’un bâtiment. Une entité à détruire ainsi qu’un bâtiment à conquérir.  
Pour le **Conquer**, l’IA va chercher la target building la plus proche découverte afin de la capturer.  
Pour le **Defend**, l’IA  va regarder toutes ses factories et checker si l’une d’entre elles est en train de se faire attaquer afin d’aller la défendre.  
Pour le **Destroy**, l’IA va sélectionner l’une des factory de l'ennemie qui a déjà vu auparavant pour en faire sa cible et l’attaquer.  
Pour le but **Explore**, on souhaite que l’IA découvre une zone qui n’a pas encore été vue par son équipe. Pour ce faire, il va prendre un point aléatoire sur la map et vérifier si cette position est atteignable et si elle n’a pas été encore vue. Sinon, on prend un autre point jusqu’à remplir ces deux conditions (on fait cette opération 5 fois, maximum).  
Pour le **Flee**, l’IA sélectionne une de ses factories afin d’y rapatrier ses troupes pour fuire.  
Pour le **Secure**, l’IA sélectionne une des target building qu’il a capturé, créer différent waypoint afin de patrouiller autour pour le sécuriser.  
Pour le **Repair**, l’IA va regarder si une de ses factories ou une de ses unités a perdu de la vie afin de les réparer. 

## Squad

Elle permet de gérer les différentes formations telles que : 
* Unstructured;
* Line; 
* Column;
* Square;
* Triangle.

Chaque squad a sa formation, si on la désélectionne et la re-sélectionne : elle gardera sa formation. Lorsqu’on modifie la Posture de la Squad, cela modifie la posture de chaque unité.  

## Tactical Planner 

**Goal Oriented Action Planning Planner** avec les spécifications du **Simplified Action Structure+Plus Post-uniqueness unariness Single-valuedness** ayant des états en liste de valeur binaire.

### Monde définit en 7 valeurs binaires:
* IsAtLocation;
* TargetIsCaptured;
* TargetIsDead;
* TimerIsFinish;
* SquadIsFull;
* TargetIsRepaired.

### Actions possibles: 
* Attack;
* Goto;
* Capture;
* Idle;
* Repair;
* SearchRecrue;
* CarryOn.

### But Tactique possible:
* Conquer;
* Defend;
* Destroy;
* Explore;
* Flee;
* Repair;
* Secure.

### Post/PréCondition:
{ 1, 0, 0, -1, -1, -1 } / Capture / { 1, 1, 0, -1, -1, -1 }  


La particularité du **Simplified Action Structure+Plus Post-uniqueness unariness Single-valuedness** permet un algorithme de recherche plus optimisé grâce au fait que: une valeur de mon état ne peut être changé que par 1 seul action. Et que 1 action ne peut changer que 1 seule valeur à la fois.  
Présence de la méta action CarryOn qui permet de créer des plans infinis cela permet d'éviter de rappeler le planner pour des plans qui vont être identiques.  

## Unit
Le Unit utilise une **State Machine (FSM)** qui lui permet de gérer les différentes postures et leurs transitions. 
* Passive;
* Agressive;
* Repair;

La posture “Passive” s’agit de la posture initiale. Il faut cibler les cibles à attaquer, à réparer.  
La posture "Agressive" permet d’attaquer directement l’ennemi le plus proche dans sa range.  
La posture “Repair” (disponible qu’aux Troopers) permet de réparer directement l’unité allié la plus proche dans sa range.  
Lorsque l’on donne une task à l’unité (Attack, capture, repair), l’unité va se déplacer vers l’objectif et dès qu’elle est à portée de range, exécutera la task.  

## UI

Lorsqu’on sélectionne une unité, on peut choisir sa posture en bas à gauche de l’écran. De même lorsqu’on sélectionne une squad, en plus d’avoir un menu pour choisir sa formation.  

Une minimap se situe en bas à droite et permet de voir les objectifs.
En haut à droite, le bouton x1 permet de mettre le jeu à la vitesse initiale. Le bouton x2 permet d’accélérer la vitesse du jeu par deux.
Un menu est disponible en cliquant sur “Echap”, cela permet de mettre en pause le jeu. Les boutons dans ce menu permettent de reprendre le jeu, de le recommencer et de le quitter.  

En haut au milieu, il y est indiqué le nombre de points de fabrications et le nombre de Target Buildings capturés.  

## Bonus possible
- Ajouter des postures réactionnaires (Faire face, Poursuite etc)
- Ajouter la gestion des meta task comme CarryOn dans le TacticalPlanner
- Ajouter la recherche et l'identification des entities de la TargetStruct dans le planner pour faciliter l'architecture.
- Ajout du déplacement via la minimap
- Ajout de la mise en place de waypoint pour le joueur
- Upgrade le planner Tactique pour qu'il prenne en compte les waypoints automatiquement dans son algo (éviter l'ajout via un if pour améliorer la lisibilité du code)
- permettre de l'IA versus IA
- Gestion en temps réel des formations, Ordonner les UT selon la position la plus proche dans la formation
- Faire garder la formation pendant le déplacement


## Images
![Alt text](Images\Example.png "Example") 
![Alt text](Images\Example2.png "Example") 
![Alt text](Images\UI.png "Example") 
![Alt text](Images\UI2.png "Example") 
