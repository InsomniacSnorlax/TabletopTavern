# Tabletop Tavern - Source Code

This is the C# source code for **Tabletop Tavern**, a solo indie game built in Unity.

## What this is

A released game made by one person. I'm sharing the code because I think there's genuine value in seeing what a shipped indie project actually looks like. It's not a tutorial, nor a polished open-source showcase, but the real thing with all it's bumps and bruises.

That means you'll find:

- Systems that evolved over time and show their age (Save system is gross but I dont want to touch it)
- Architectural decisions that made sense at the time and don't hold up
- A few `old/` directories
- Some genuinely good patterns mixed in with some stuff I'd do very differently today

## Honest notes

I know the code has rough spots. Imperfect code that's out in the hands of players beats perfect code that never leaves your hard drive.

## What's not here

**Assets are not included.** This means no art, audio, prefabs, or scenes. Redistributing them would violate the licensing terms of third-party assets used in the project. You will not be able to run the game from this repo alone.

What you *can* do is read through the code, study how the systems fit together, and take anything useful for your own work. Or tell me how to best rearchitect it to support mods.

## Tech overview

- **Engine:** Unity (DOTS/ECS hybrid, Unity.Entities + MonoBehaviour for UI and campaign)
- **Language:** C#
- **Architecture:** Two-scene structure (campaign map + battle); ECS handles unit simulation, MonoBehaviour handles UI and game management
- **Key systems:**
  - Battle simulation (ECS units, formations, ranged/melee combat)
  - Campaign map with procedural node generation
  - Spell system
  - Gear and hero progression
  - Localization via a custom `LocalizationManager`
  - Steam integration (Steamworks)
  - Pooled audio/SFX manager

## Assembly structure

The code is split across a few namespaces/assemblies:

| Assembly | Contents |
| --- | --- |
| `TabletopTavern.Core` | Main game logic — battle, campaign, UI, ECS |
| `Memori.Audio` | Audio singleton and BGM player |
| `Memori.Utilities` | Shared utility code |
| `TJ.IrregularGrid` | Procedural map generation (WFC-based) |
