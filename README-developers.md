How to release a new version
===

1. Update `VersionPrefix` and package release notes in [NLog.Windows.Forms.csproj](NLog.Windows.Forms/NLog.Windows.Forms.csproj) according to semantic versioning. Change `AssemblyVersion` only for a major release.
2. Wait for the GitHub Actions workflow to pass.
3. Build and publish the NuGet package through the project's release process; CI does not publish packages.
4. Create a GitHub release with a tag starting with `v` and include the changelog.
5. Close the GitHub milestone.
