# Installation

## Building from Source

```bash
git clone <repository-url>
cd GeminiWebApiDotnet
dotnet build
```

## Download from Release

> **Note:** This library is not published on NuGet.org. Download from the GitHub Release page.

### Manual DLL and Dependency Resolution

**Step 1:** Download the following files from the [Releases page](https://github.com/toretate/GeminiApiDotNet/releases)
- `GeminiWebApi-X.X.X.dll`
- `GeminiWebApi-X.X.X.pdb` (Optional: for debug information)

**Step 2:** Place the DLL in your project (example: `lib` folder)

```
YourProject/
├── lib/
│   ├── GeminiWebApi.dll
│   └── GeminiWebApi.pdb
├── YourProject.csproj
└── ...
```

**Step 3:** Add DLL reference to your project's `.csproj` file

```xml
<ItemGroup>
  <Reference Include="..\lib\GeminiWebApi.dll" />
</ItemGroup>
```

**Step 4:** Install dependency packages via NuGet

```bash
dotnet add package System.Net.Http --version 4.3.4
dotnet add package System.Text.Json --version 8.0.0
dotnet add package HttpClientFactory --version 1.0.0
```

Or add directly to `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="System.Net.Http" Version="4.3.4" />
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
  <PackageReference Include="HttpClientFactory" Version="1.0.0" />
</ItemGroup>
```

**Step 5:** Restore and build

```bash
dotnet restore
dotnet build
```
