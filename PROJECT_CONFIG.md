# Project Configuration

## .NET CLI コマンド

### ビルド
```bash
dotnet build
dotnet build -c Release
```

### テスト
```bash
dotnet test
```

### パッケージ化
```bash
dotnet pack -c Release
```

### 実行
```bash
# ライブラリのビルド
dotnet build src/GeminiWebApi/GeminiWebApi.csproj

# テスト実行
dotnet test tests/GeminiWebApi.Tests.csproj

# サンプル実行
dotnet run --project example/GeminiWebApi.Examples.csproj
```

## 環境変数

`.env` ファイルを作成して以下を設定：

```
GEMINI_PSID=YOUR_SECURE_1PSID
GEMINI_PSIDTS=YOUR_SECURE_1PSIDTS
GEMINI_PROXY=optional_proxy_url
```

## ターゲットフレームワーク

- .NET 8.0

## 必要な NuGet パッケージ

```
dotnet add package System.Net.Http
dotnet add package System.Text.Json
dotnet add package HttpClientFactory

# テスト用
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
```

## ローカル開発セットアップ

1. リポジトリをクローン
2. `dotnet restore` を実行
3. `dotnet build` でビルド
4. `dotnet test` でテスト実行
5. 自分のクッキー情報を設定して例を実行

## パッケージ構成

- `GeminiWebApi` - メインライブラリ
- `GeminiWebApi.Tests` - ユニットテスト
- `GeminiWebApi.Examples` - 使用例
