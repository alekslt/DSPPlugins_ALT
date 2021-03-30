# Configuration

This part is included by the best of the MSBuild items to define the user specific information.

`Game.props` handles information relating to the game install.

`Repo.props` handles publishing information.

`Unity.props` handles currently unused properties for Unity itself - i.e. reference DLLs from Unity, not the game itself.

These are imported by `../msbuild/Config.props`, and are defined for the rest of the items.