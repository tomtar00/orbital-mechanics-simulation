# orbital-mechanics-simulation
An orbital mechanics simulation/visualisation made with Unity.
This project aims to provide a simple implementation of a basic space maneuver system (with autopilot) and show how much time space travel takes in the Solar System. All data is as close to the real values as possible.

### How to use
1. Create a spacecraft on an orbit around given celestial body at a specific time.
![main](.github/images/main.png)

2. Create maneuvers pressing on spacecraft's orbit/tracjectory.
![1](.github/video/1.mp4)
3. Modify maneuvers by pressing arrows that represent change to velocity in the marked direction
![2](.github/video/2.mp4)
4. You can also drag the entire maneuver node along the trajectory line
![3](.github/video/3.mp4)
5. Use autopilot or rotate the spacecraft towards the final maneuver direction marked as an arrow near the spacecraft
![4](.github/video/4.mp4)
6. Wait for autopilot to make the burn or do it yourself when spacecraft is near the maneuver node
![5](.github/video/5.mp4)
7. You can also create multiple maneuver nodes that relay on future trajectories
![6](.github/video/6.mp4)

### Controls
- `Q` - unlock cursor (press in empty space to lock cursor)
- `W` `A` `S` `D` - rotate spacecraft
- `Space` - add acceleration