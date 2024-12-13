# Underwater-RL

Remember to adjust observation vector size, position & size of obstacles and agent, ray sensor parameters, install Linux packages

# Telerobotics and HRI - Simulation Server

## Overview
This repository contains the simulation server project for the **Telerobotics and Human-Robot Interaction (HRI)** course, developed as part of the LAB1 exercise. The project focuses on building a simulation server using **Unity** and **ROS** for robotic system development and control. The server allows for real-time simulation and teleoperation of a robot in a custom-designed environment.

## Table of Contents
- [Project Structure](#project-structure)
- [Task Overview](#task-overview)
- [Simulation Setup](#simulation-setup)
  - [Scene Design](#scene-design)
  - [Water Plane Setup](#water-plane-setup)
- [Unity and ROS Integration](#unity-and-ros-integration)
- [Teleoperation of BlueRov Robot](#teleoperation-of-bluerov-robot)
- [Useful Links](#useful-links)
- [How to Run](#how-to-run)
- [License](#license)

---

## Project Structure
The project is organized into the following directories:
- **Assets/**: Contains Unity assets, including 3D models, scripts, and materials.
- **Library/**: Contains Unity-generated files (ignored in version control).
- **Packages/**: Unity package information.
- **ProjectSettings/**: Unity project settings.
- **UserSettings/**: Local user settings.

Note: The `PackageCache` folder is ignored from version control to reduce repository size.

## Task Overview
This project implements several tasks as outlined in the course, including the integration of **Unity** and **ROS**, the setup of a simulation environment, and teleoperation capabilities. Below is a breakdown of the key tasks:

1. **Scene Design**:
   - Created a custom pool environment using **Unity** for simulating water-based robotics.
   - Utilized **HDRP (High Definition Rendering Pipeline)** for realistic water rendering and environment simulation.
   - Imported 3D models and assets from Unity’s Asset Store and OnShape for scene creation.

2. **Water Plane Setup**:
   - Explored options for water plane simulation using **Aquas**, **Gaia**, and other assets.
   - Added a water plane using Unity's High Definition Rendering Pipeline (HDRP) and configured the settings for realistic water behavior.

3. **Unity and ROS Integration**:
   - Installed **ROS TCP Connector** to integrate Unity with ROS.
   - Imported **URDF** model of the **BlueRov** underwater robot.
   - Adjusted model properties to correctly align the BlueRov in the simulation.
   - Configured camera views for top-down and underwater perspectives.

4. **Teleoperation of BlueRov Robot**:
   - Installed **ROS URDF Importer** to import robot models and integrate them with the Unity scene.
   - Used **ROS-TCP Endpoint** to enable communication between Unity and ROS.
   - Set up **teleoperation** using ROS’s `teleop_twist_keyboard` package, allowing control of the BlueRov robot through keyboard commands.
   - Scripted the robot's movement in the Unity environment, enabling real-time control and camera visualization.

---

## Simulation Setup

### Scene Design
- **Unity Version**: 2022.2 or later (HDRP enabled)
- **3D Tools**: OnShape and Unity Blocks for modeling the pool environment.
- **Assets**: Various models from Unity Asset Store, including water simulation packages like Aquas and Gaia.

### Water Plane Setup
- Added a water plane in Unity by navigating to `Assets > Create > Water Plane`.
- Adjusted water plane properties (height, material) to simulate underwater effects and realistic fluid dynamics.
- Verified that the **HDRP Wizard** was correctly configured for rendering water.

---

## Unity and ROS Integration

### ROS TCP Connector Installation
1. Installed **ROS TCP Connector** from GitHub:
   - `https://github.com/Unity-Technologies/ROS-TCP-Connector.git`
2. Configured the ROS workspace to include the ROS TCP endpoint:
   ```bash
   ros2 launch ros_tcp_endpoint endpoint.launch
   ros2 launch ros_tcp_endpoint endpoint.py
   ```
3. Connected Unity to the ROS environment, enabling real-time data exchange between the simulation and the robot control system.

### URDF Import and Robot Setup
- Imported the **BlueRov** URDF model from `bluerov_ros_playground`.
- Converted the Xacro files to URDF using the Xacro tool and placed them in the Unity `Assets/Models` directory.
- Adjusted the model’s properties, such as orientation and gravity, to ensure correct placement in the simulation.

---

## Teleoperation of BlueRov Robot

### Setup
1. Installed and configured **teleop_twist_keyboard** for ROS1 and ROS2:
   - ROS1: `https://github.com/ros-teleop/teleop_twist_keyboard.git`
   - ROS2: `https://github.com/ros2/teleop_twist_keyboard.git`
2. Scripted Unity to subscribe to ROS velocity commands (`cmd_vel`) and applied them to the BlueRov model.
```bash
ros2 run teleop_twist_keyboard teleop_twist_keyboard --ros-args --remap cmd_vel:=/bluerov1/cmd_vel
```
3. Adjusted camera viewports to display multiple perspectives, including top-down and underwater views.

### Camera Integration
- Added a camera to the BlueRov GameObject.
- Configured the camera’s **View Ports** to provide multiple views in Unity's game window.
- Visualized camera topics via **RVIZ** or another ROS visualization tool.

---

## Useful Links
- [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub)
- [BlueRov URDF Model](https://github.com/patrickelectric/bluerov_ros_playground)
- [OnShape Tutorials](https://learn.onshape.com/catalog)
- [OnShape-to-Robot Documentation](https://onshape-to-robot.readthedocs.io/en/latest/)

---

## How to Run

### Prerequisites
- Unity Editor (version 2022.2 or higher with HDRP)
- ROS (ROS1 or ROS2)
- ROS TCP Connector installed in both Unity and ROS workspace

### Steps to Run the Simulation:
1. Clone the repository:
   ```bash
   git clone https://github.com/eather0056/Telerobotics-and-HRI-Simulation-Server.git
   ```
2. Open the project in Unity.
3. Ensure **ROS TCP Connector** is running on the ROS side:
   ```bash
   roslaunch ros_tcp_endpoint endpoint.launch
   ```
4. Run the Unity simulation to control the BlueRov robot using the ROS `teleop_twist_keyboard`.

---

## License
This project is licensed under the **GPL-3.0 License**. See the [LICENSE](LICENSE) file for details.