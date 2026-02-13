# GeminiWebApi - C# Gemini API Wrapper

<img src="https://raw.githubusercontent.com/HanaokaYuzu/Gemini-API/master/assets/logo.svg" width="35px" alt="Gemini Icon" /> 

C# reverse-engineered wrapper for [Google Gemini](https://gemini.google.com/) web app.

このプロジェクトは下記の Python プロジェクトの C# 版を作成することを目的としています
https://github.com/HanaokaYuzu/Gemini-API

**ベースバージョン:**
- **Repository**: https://github.com/HanaokaYuzu/Gemini-API
- **Commit**: `3f2b935420fd688e1ab84d937de2c33a871697c0`
- **Date**: 2026-02-10
- **Message**: Fix the retry logic for unsaved conversations. (#233)

## 概要

元の [Python Gemini API](https://github.com/HanaokaYuzu/Gemini-API) プロジェクトの C# ポートです。以下の機能をサポートする Google Gemini Web API 用の非同期第一の包装提供します：

- **永続的クッキー** - クッキーの自動管理と更新
- **画像生成** - 画像の生成と編集をサポート
- **システムプロンプト** - Gems を使用したカスタマイズ可能なプロンプト
- **拡張機能サポート** - Gemini 拡張機能との統合
- **ストリーミングモード** - リアルタイムコンテンツ生成
- **マルチターン会話** - 組み込みチャットセッション管理
- **非同期** - .NET の async/await サポート完全装備

## 要件

- .NET 8.0 以上
- C# 11 以上
- Gemini へのアクセスがある有効な Google アカウント

## インストール

### ソースからビルド

```bash
git clone <repository-url>
cd GeminiWebApiDotnet
dotnet build
```

## 認証

1. [https://gemini.google.com](https://gemini.google.com/) にアクセスしてログイン
2. 開発者ツール (F12) を開き、Network タブに移動
3. ページを更新
4. リクエストをクリックし、以下のクッキー値をコピー：
   - `__Secure-1PSID`
   - `__Secure-1PSIDTS` (オプション)

## クイックスタート

### 基本的な使用方法

```csharp
using GeminiWebApi;
using GeminiWebApi.Utils;

Logger.SetLogLevel(Logger.LogLevel.Info);

var client = new GeminiClient(
    securePsid: "YOUR_SECURE_1PSID",
    securePsidts: "YOUR_SECURE_1PSIDTS"
);

await client.InitializeAsync(timeout: 30);

var response = await client.GenerateContentAsync("Hello World!");
Console.WriteLine(response.Text);

await client.CloseAsync();
```

### ストリーミング

```csharp
await foreach (var chunk in client.GenerateContentStreamAsync("Tell me a story"))
{
    Console.Write(chunk.TextDelta);
}
```

### マルチターン会話

```csharp
var chat = client.StartChat();

var response1 = await chat.SendMessageAsync("What is machine learning?");
Console.WriteLine(response1.Text);

var response2 = await chat.SendMessageAsync("Can you give me an example?");
Console.WriteLine(response2.Text);
```

### モデル選択

```csharp
using GeminiWebApi.Constants;

var response = await client.GenerateContentAsync(
    "What model are you?",
    model: ModelType.Gemini30Flash
);
```

利用可能なモデル：
- `ModelType.Unspecified` - デフォルトモデル
- `ModelType.Gemini30Pro` - Gemini 3.0 Pro
- `ModelType.Gemini30Flash` - Gemini 3.0 Flash  
- `ModelType.Gemini30FlashThinking` - Gemini 3.0 Flash Thinking

### Gems パネル (システムプロンプト)

```csharp
// 利用可能な Gems を取得
var gems = await client.FetchGemsAsync(includeHidden: false);

// 特定の Gem を検索
var codingGem = gems.Get(name: "Coding Helper");

// カスタム Gem を作成
var customGem = await client.CreateGemAsync(
    name: "Python Expert",
    prompt: "You are an expert Python developer",
    description: "Helps with Python programming"
);

// 会話で Gem を使用
var chat = client.StartChat(gem: customGem.Id);
var response = await chat.SendMessageAsync("How do I use decorators?");
```

### 画像処理

```csharp
// 画像を生成
var response = await client.GenerateContentAsync("Generate an image of a sunset");

// レスポンス内の画像にアクセス
foreach (var image in response.Images)
{
    Console.WriteLine($"Image: {image.Title}");
    await image.SaveAsync(path: "./images/", filename: "sunset.png");
}
```

## エラーハンドリング

```csharp
using GeminiWebApi.Exceptions;

try
{
    var response = await client.GenerateContentAsync("Hello");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"認証失敗: {ex.Message}");
}
catch (ModelInvalidException ex)
{
    Console.WriteLine($"無効なモデル: {ex.Message}");
}
catch (GeminiException ex)
{
    Console.WriteLine($"エラー: {ex.Message}");
}
```

## ロギング

ロギング出力を制御：

```csharp
using GeminiWebApi.Utils;

Logger.SetLogLevel(Logger.LogLevel.Debug);      // 詳細ログ
Logger.SetLogLevel(Logger.LogLevel.Info);       // 情報レベル
Logger.SetLogLevel(Logger.LogLevel.Warning);    // 警告のみ
Logger.SetLogLevel(Logger.LogLevel.Error);      // エラーのみ
```

## API リファレンス

### GeminiClient

Gemini API と相互作用するためのメインクラス。

**メソッド:**
- `InitializeAsync()` - クライアントを初期化
- `GenerateContentAsync()` - プロンプトからコンテンツを生成
- `GenerateContentStreamAsync()` - ストリーミングでコンテンツ生成
- `StartChat()` - 新しいチャットセッションを作成
- `FetchGemsAsync()` - 利用可能な Gems を取得
- `CreateGemAsync()` - カスタム Gem を作成
- `CloseAsync()` - クライアントを閉じてクリーンアップ

### ChatSession

会話セッションを表します。

**メソッド:**
- `SendMessageAsync()` - チャットでメッセージを送信
- `SendMessageStreamAsync()` - ストリーミングでメッセージを送信
- `ChooseCandidate()` - 特定のレスポンス候補を選択

### ModelOutput

コンテンツ生成の結果。

**プロパティ:**
- `Text` - 生成されたテキスト全体
- `TextDelta` - 最後のチャンク以降の新しいテキスト（ストリーミング用）
- `Thoughts` - モデルの思考プロセス（利用可能な場合）
- `Images` - レスポンス内の画像リスト

## プロジェクト構造

```
GeminiWebApiDotnet/
├── src/GeminiWebApi/
│   ├── Constants/          # API 定数とエンドポイント
│   ├── Exceptions/         # カスタム例外
│   ├── Models/             # データモデル
│   ├── Utils/              # ユーティリティ関数
│   └── GeminiClient.cs     # メインクライアントクラス
├── example/                 # サンプル使用法
├── tests/                   # ユニットテスト
├── GeminiWebApi.csproj     # プロジェクトファイル
└── ReadMe.md
```

## ライセンス

AGPL-3.0 ライセンス - 詳細は LICENSE ファイルを参照

## 注記

このプロジェクトは逆流エンジニアリングの実装であり、Google とは提携していません。ご自身のリスクと Google の利用規約に準拠してご使用ください。