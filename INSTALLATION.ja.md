# インストール方法

## ソースからビルド

```bash
git clone <repository-url>
cd GeminiWebApiDotnet
dotnet build
```

## Release からダウンロード

> **注意：** このライブラリは NuGet.org に公開されていません。GitHub の Release ページからダウンロードして使用してください。

### DLL と依存関係の手動解決

**ステップ 1:** [Releases ページ](https://github.com/toretate/GeminiApiDotNet/releases) から以下のファイルをダウンロード
- `GeminiWebApi-X.X.X.dll`
- `GeminiWebApi-X.X.X.pdb` (オプション：デバッグ情報用)

**ステップ 2:** プロジェクトに DLL を配置（例：`lib` フォルダ）

```
YourProject/
├── lib/
│   ├── GeminiWebApi.dll
│   └── GeminiWebApi.pdb
├── YourProject.csproj
└── ...
```

**ステップ 3:** プロジェクトの `.csproj` ファイルに DLL 参照を追加

```xml
<ItemGroup>
  <Reference Include="..\lib\GeminiWebApi.dll" />
</ItemGroup>
```

**ステップ 4:** 依存パッケージを NuGet でインストール

```bash
dotnet add package System.Net.Http --version 4.3.4
dotnet add package System.Text.Json --version 8.0.0
dotnet add package HttpClientFactory --version 1.0.0
```

または、`.csproj` に直接追加：

```xml
<ItemGroup>
  <PackageReference Include="System.Net.Http" Version="4.3.4" />
  <PackageReference Include="System.Text.Json" Version="8.0.0" />
  <PackageReference Include="HttpClientFactory" Version="1.0.0" />
</ItemGroup>
```

**ステップ 5:** 復元とビルド

```bash
dotnet restore
dotnet build
```
