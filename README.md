# roadbuilder
Build road just like in a simcity game.

<img src="https://github.com/guotata1996/guotata1996.github.io/raw/master/img/post3/0113.gif" width="500" height="322" align=center />

## Building tools
- Create/delete roads of 3 curve types: straight line / arc / bezeir curve
- Round and smooth intersections
- Intelligent and easy drawing
- Support elevated road and 3D objects on road side
- Input validity check

## Traffic Simulation
- Navigation from source to sink with auto lane selection
- Longitudinal and lane-changing behavior based on IDM and BOMIL models

## Plans
- Refactor backend codes for better maintainability & speedup. 
    - Create Unit tests for complicated classes.
    - Mainscene would still be working with old scipts kept in `Old/` Folders.
- Integrate Unity ECS into traffic system.
- Handle lane-changing deadlocks

## Demo
[![Vehicle](https://img.youtube.com/vi/m6vOuXqUa0A/0.jpg)](https://www.youtube.com/watch?v=m6vOuXqUa0A)
