# GuildMaster (UPM package)

All setup done for the GuildMaster UPM package.

Version: 1.0.1

What is included
- Runtime/ (package runtime sources)
  - Core/ (domain entities, enums, value objects)
  - Systems/ (game systems)
- Single assembly definition: Runtime/GuildMaster.asmdef (rootNamespace: GuildMaster)
- package.json (name: com.ssstudio.guildmaster)

How to use
- Local path:
  1) In Unity Editor: Window → Package Manager → + → Add package from disk...
  2) Select this folder's `package.json` file.
- Git URL:
  Use the repository Git URL in Package Manager → + → Add package from Git URL...

Notes
- The package exposes namespaces under `GuildMaster.Core.*` and `GuildMaster.Systems.*`.
- The package contains runtime code only (under `Runtime/`). No nested asmdefs are present — a single `Runtime/GuildMaster.asmdef` provides the compiled assembly.
- If your IDE shows namespace→file-location warnings, import the package into Unity (Unity is the authoritative compiler for packages). These warnings are workspace/IDE specific and not Unity compile errors.

If you want
- I can push this package to a release tag (v1.0.1) on the repository and show how to pin the package by tag in Unity's Package Manager.
- I can create a small example scene demonstrating usage (Calculator UI or a small demo).
