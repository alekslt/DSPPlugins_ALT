REM Prepare Thunderstore Release
echo - %1 - %2 - 
set ProjectName=%1
set SolutionDir=%2
set TargetDir=%3

set ReleasePath=%SolutionDir%\..\Releases
set ProjectReleasePath=%SolutionDir%\..\Releases\alekslt-%ProjectName%-v0.0
IF NOT EXIST %ProjectReleasePath%\%ProjectName% MKDIR %ProjectReleasePath%\%ProjectName%
copy /y "%TargetDir%%ProjectName%.dll" "%ProjectReleasePath%\%ProjectName%"
copy /y "%TargetDir%*.md" "%ProjectReleasePath%\"
copy /y "%TargetDir%Thunderstore\*.*" "%ProjectReleasePath%\"
