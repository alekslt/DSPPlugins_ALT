# Build Infrastructure

The `Directory.Build.*` handle importing the MSBuild projects here. See [MSBuild docs](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-concepts?view=vs-2019) for a lot more in-depth explanations.

### Config

`NerdBank.GitVersioning` handles automatic version increment based on `src/version.json` + each project's `version.json`.

The values are used in other places like `Thunderstore`. Run `dotnet tool install -g nbgv` for that to work.

### BepInEx

`BepInEx.props` handles project output in plugins and referencing `BepInEx` core libraries.

`BepInEx.targets` handles resolving plugin references from locations like game install before resolving assemblies to build against.

See `PluginReference` for plugin resolution.

### Thunderstore

`Thunderstore.props` includes the `BepInEx` reference and default icon handling.

`Thunderstore.targets` handles generating the manifest and the resulting package zip after the build.

See `Thunderstore` ItemGroup for items which are only copied on publishing.


### Unity

`Unity.props` handles Unity specific information, like game directory assembly paths and .NET 3.5 handling.

`Unity.targets` handles publishing the hand-filled `Assembly-CSharp` docs and running `pdb2mdb` for debug symbols after the build.

NOTE: Unity handling for non-net35 and backups of assemblies for each version is planned.

### Simplified build graph

[![](https://mermaid.ink/img/eyJjb2RlIjoiZ3JhcGggVERcbiAgICBBW01TQnVpbGRdIC0tPnxCdWlsZHwgQihEU1BNb2RzLnNsbilcbiAgICBCIC0tPiBDKE9yZGVyU2F2ZXMuY3Nwcm9qKVxuICAgIEIgLS0-ICouY3Nwcm9qXG5cbiAgICBDIC0tPiBEKFNESy5wcm9wcylcbiAgICBEIC0tPiBFKERpcmVjdG9yeS5CdWlsZC5wcm9wcylcbiAgICBFIC0tPiBGKG1zYnVpbGQvKi5wcm9wcylcbiAgICBGIC0tPiBHKFNESy50YXJnZXRzKVxuICAgIEcgLS0-IEgoRGlyZWN0b3J5LkJ1aWxkLnRhcmdldHMpXG4gICAgSCAtLT4gSShtc2J1aWxkLyoudGFyZ2V0cylcbiIsIm1lcm1haWQiOnsidGhlbWUiOiJkZWZhdWx0In0sInVwZGF0ZUVkaXRvciI6ZmFsc2V9)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoiZ3JhcGggVERcbiAgICBBW01TQnVpbGRdIC0tPnxCdWlsZHwgQihEU1BNb2RzLnNsbilcbiAgICBCIC0tPiBDKE9yZGVyU2F2ZXMuY3Nwcm9qKVxuICAgIEIgLS0-ICouY3Nwcm9qXG5cbiAgICBDIC0tPiBEKFNESy5wcm9wcylcbiAgICBEIC0tPiBFKERpcmVjdG9yeS5CdWlsZC5wcm9wcylcbiAgICBFIC0tPiBGKG1zYnVpbGQvKi5wcm9wcylcbiAgICBGIC0tPiBHKFNESy50YXJnZXRzKVxuICAgIEcgLS0-IEgoRGlyZWN0b3J5LkJ1aWxkLnRhcmdldHMpXG4gICAgSCAtLT4gSShtc2J1aWxkLyoudGFyZ2V0cylcbiIsIm1lcm1haWQiOnsidGhlbWUiOiJkZWZhdWx0In0sInVwZGF0ZUVkaXRvciI6ZmFsc2V9)

Each csproj is really lightweight, with just Title, Description and README.md definitions, and some extra content where applicable, like `icon` in `WhatTheBreak`.
