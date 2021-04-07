```
USAGE: Version [--help] [--version] [--set-version <version>] [--major] [--minor] [--patch] [--subpatch] [--dont-add] [--prerelease <string>] [<string>...]

PROJECTS:

    <string>...           Gets the project folders to look in for version information.

OPTIONS:

    --version, -v         This will show the current version (2.0.0.0 built at 24 Mar 2021 21:49:33 UTC) of the application
    --set-version, -set-version <version>
                          Allows you to set the version to a specific value. This version prevents any other version commands from running. This will
                          always create a new version if none are present.
    --major, -major       This argument will increment the major version number. This will cause Minor, Patch, and Sub Patch to be set to 0.
    --minor, -minor       This argument will increment the minor version number. This will cause Patch and Sub Patch to be set to 0.
    --patch, -patch       This argument will increment the patch version number. This will cause Sub Patch to be set to 0.
    --subpatch, -subpatch, -buildNum
                          This argument will increment the sub-patch version or build number.
    --dont-add, -nonew    Prevents the initialization of version for projects that do not have a version.
    --prerelease, -pre, -prerelease <string>
                          Sets the pre-release identifier, causing the library to be a prerelease.
    --help                display this list of options.
```