# Stove Mod for Vintage Story

A stove block for Vintage Story that functions as an enclosed cooking station.

## Features

- Enclosed stove with 1.25x heat efficiency
- Supports cooking with pots (animated lid while cooking)
- Only accepts charcoal, anthracite, and black coal as fuel
- Available in 20 different rock rim variants
- Displays cooking progress and temperature

## Crafting Recipe

```
S I S
F _ F
F F F
```

- S = Rock (determines rim variant)
- I = Iron Plate
- F = Fire Brick

## Building from Source

1. Ensure you have .NET 8.0 SDK installed
2. Set your Vintage Story installation path in the `.csproj` if needed
3. Build the project:
   ```
   dotnet build -c Release
   ```
4. Copy `StoveMod.dll` and the `assets` folder to your mods folder

## Installation

1. Download the latest release
2. Place the mod zip file in your Vintage Story `Mods` folder
3. Start the game

## License

MIT License
