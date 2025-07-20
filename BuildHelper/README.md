# Plugin Build Helper

Creates a `manifest.json` file based on info set in the `.csproj` file and exposes those values in your code via the `ManifestInfo` type.

### Example properties in `.csproj`:
```xml
<PropertyGroup>
    <TSName>ModNameHere</TSName>
    <TSAuthor>AuriRex</TSAuthor>
    <TSVersion>0.0.1</TSVersion> <!-- TSVersion empty = GitInfo used instead (if available; check nuget) -->
    <TSWebsite>https://github.com/AuriRex/GTFO_TheArchive</TSWebsite>
    <TSDescription>Mod Description goes here.</TSDescription>
</PropertyGroup>
<ItemGroup>
    <!-- Add the Dependency strings here, one for each dependency -->
    <!-- Clonesoft is just here as an example, remove/replace that one if you don't need it! -->
    <TSDependencyStrings Include="BepInEx-BepInExPack_GTFO-3.2.1" InProject="false" />
    <TSDependencyStrings Include="AuriRex-Clonesoft_Json-13.0.0" InProject="false" />
</ItemGroup>
```

### Source files:

* Source Generator: https://github.com/AuriRex/GTFO_TheArchive/tree/master/ManifestInfoSourceGenerator
* Manifest Generator Task: https://github.com/AuriRex/GTFO_TheArchive/tree/master/BuildTasks