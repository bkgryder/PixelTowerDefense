# PixelTowerDefense

PixelTowerDefense is a small MonoGame sample that demonstrates simple physics-based interactions with pixel characters. Enemies walk around a bounded arena and can be grabbed and tossed by the mouse, exploding into particles when thrown from great heights.

## Building

This project targets **.NET 8**. To build it locally run:

```bash
# Restore tools and packages
dotnet restore

# Build the solution
 dotnet build PixelTowerDefense.sln
```

## Running

You can launch the game directly with `dotnet run` from the project directory:

```bash
cd PixelTowerDefense
 dotnet run
```

The game window will open with a simple arena populated by enemies.

## Controls

- **WASD** – Pan the camera
- **Plus / Minus** – Zoom in or out
- **Left mouse** – Grab an enemy by one of its parts. Release to launch it
- **P** – Spawn a new enemy
- **Esc** – Quit the game

## Core Features

- **AI System** – Handles wandering behaviour and recovering from stun
- **Physics System** – Simulates gravity, collisions and launching enemies
- **Input System** – Manages camera movement and mouse interactions
- **Render System** – Draws the arena, enemies and explosion particles

This code base is intentionally small and self contained, making it suitable as a starting point for further experiments or tutorials.
