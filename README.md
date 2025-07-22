# PixelTowerDefense

PixelTowerDefense is a small MonoGame project demonstrating a simple tower defense style game made from pixel art.

## Features

- Trees with random trunks and leafy canopies spawn around the arena. Meeples are pushed away when they hit a trunk.

## Prerequisites

- .NET 8.0 SDK
- MonoGame Framework (DesktopGL) 3.8.2 (restored via NuGet)

The repo includes a `.config/dotnet-tools.json` file with MonoGame content builder tools. Restore them with:

```bash
dotnet tool restore
```

## Building

Run the following from the repository root to compile the game and its content:

```bash
dotnet build PixelTowerDefense.sln -c Release
```

This will build the `PixelTowerDefense` project and place the binaries in `PixelTowerDefense/bin/Release/net8.0`.

## Running

Execute the game using `dotnet run`:

```bash
dotnet run --project PixelTowerDefense/PixelTowerDefense.csproj
```

MonoGame requires an available desktop environment. If running headless, the game may fail to start.

