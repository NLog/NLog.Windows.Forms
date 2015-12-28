How to release a new version
===

1. Change `version` and `assembly_informational_version` in [appveyor.yml](appveyor.yml), according to [semver](semver.org). 
 - Only change `assembly_version` which each major version! 
2. Wait for build completion on [AppVeyor](https://ci.appveyor.com).
3. Push to NuGet by in AppVeyor:
  1. Chooser "deployments"
  2. Hover "Nuget.org & SymbolSource.org" and press "update"
  3. Hover proper build and press "publish"
4. Check/update changelog in NuGet (if you have proper rights)   
5. Create a GitHub release.   (tag should start with `v`)
