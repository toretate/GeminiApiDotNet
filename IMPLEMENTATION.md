# GeminiWebApi - C# Gemini API ラッパー

完全な C# 実装ガイド

## プロジェクト構造

```
GeminiWebApiDotnet/
├── src/
│   └── GeminiWebApi/
│       ├── Constants/
│       │   └── ApiConstants.cs       # API定数、エンドポイント、モデル定義
│       ├── Exceptions/
│       │   └── GeminiExceptions.cs   # カスタム例外クラス
│       ├── Models/
│       │   └── DataModels.cs         # データモデル（Image, Gem, ModelOutput等）
│       ├── Utils/
│       │   └── UtilityClasses.cs     # ユーティリティ関数
│       ├── GeminiClient.cs           # メインクライアントとChatSession
│       └── GeminiWebApi.csproj       # ライブラリプロジェクトファイル
│
├── tests/
│   ├── GeminiClientTests.cs          # ユニットテスト
│   └── GeminiWebApi.Tests.csproj     # テストプロジェクトファイル
│
├── example/
│   ├── Program.cs                   # 使用例
│   └── GeminiWebApi.Examples.csproj # 例プロジェクトファイル
│
├── GeminiWebApi.sln                 # ソリューションファイル
├── GeminiWebApi.csproj              # ルートプロジェクトファイル
├── ReadMe.md                        # メインREADME
├── IMPLEMENTATION.md                # 実装ガイド
└── .gitignore                       # git設定
```

## 主な機能実装

### 1. GeminiClient クラス

メインのクライアントクラスで、以下を管理：
- HTTP通信（HttpClient）
- クッキー管理
- セッション ID の追跡
- リクエスト ID の管理

#### 主なメソッド：

```csharp
// 初期化
Task InitializeAsync(int timeout, bool autoClose, int closeDelay, bool autoRefresh)

// コンテンツ生成
Task<ModelOutput> GenerateContentAsync(string prompt, List<string>? files, ModelType model, string? gem, ChatSession? chat)

// ストリーミング生成
IAsyncEnumerable<ModelOutput> GenerateContentStreamAsync(...)

// チャットセッション開始
ChatSession StartChat(ModelType model, string? gem, ChatSessionMetadata? metadata)

// Gems管理
Task<GemJar> FetchGemsAsync(bool includeHidden, string language)
Task<Gem> CreateGemAsync(string name, string prompt, string description)

// バッチ実行
Task<HttpResponseMessage> BatchExecuteAsync(List<RpcData> rpcCalls)

// リソース解放
Task CloseAsync()
```

### 2. ChatSession クラス

会話セッションを表現：
- メタデータ管理（ConversationId, ResponseId等）
- マルチターン会話の状態追跡

#### 主なメソッド：

```csharp
// メッセージ送信
Task<ModelOutput> SendMessageAsync(string message, List<string>? files)

// ストリーミング送信
IAsyncEnumerable<ModelOutput> SendMessageStreamAsync(string message, List<string>? files)

// 候補選択
void ChooseCandidate(int index)
```

### 3. データモデル

#### Image クラス群
- `Image` - 基底クラス
- `WebImage` - Web上の画像
- `GeneratedImage` - AI生成画像

各画像型は`SaveAsync`メソッドで保存可能。

#### ModelOutput
API レスポンスのメインモデル：
- `Text` - 生成されたテキスト
- `TextDelta` - 差分テキスト（ストリーミング用）
- `Thoughts` - モデルの思考プロセス
- `Images` - 画像リスト
- `Candidates` - 代替案リスト

#### Gem と GemJar
- `Gem` - システムプロンプト定義
- `GemJar` - Gems コレクション（フィルタリング/検索機能付き）

### 4. ユーティリティ

#### JsonUtils
- `GetNestedValue()` - JSON構造内の値取得
- `Serialize()` / `Deserialize()` - JSON変換

#### TextUtils
- `GetCleanText()` - テキストクリーニング
- `GetDeltaByText()` - テキスト差分計算

#### Logger
- `SetLogLevel()` - ログレベル設定
- `Debug()`, `Info()`, `Warning()`, `Error()`, `Critical()` - ログ出力

#### など

### 5. 定数と設定

#### ApiConstants.cs
- `Endpoints` - APIエンドポイント
- `GrpcRoutes` - gRPC ルートID
- `RequestHeaders` - デフォルトHTTPヘッダー
- `Model` - モデル定義
- `ErrorCode` - エラーコード列挙型

### 6. 例外処理

カスタム例外の階層：
```
GeminiException (基底)
├── AuthenticationException
├── ApiException
├── ModelInvalidException
├── UsageLimitExceededException
├── TemporarilyBlockedException
└── TimeoutException
```

## 移植用の Python ↔ C# マッピング

| Python | C# |
|--------|-----|
| `async def` | `async Task` / `async IAsyncEnumerable` |
| `asyncio` | `Task`, `await`, `IAsyncEnumerable` |
| `dict` | `Dictionary<K, V>` / `object` |
| `list` | `List<T>` |
| `@property` | Property with get/set |
| `Enum` | `enum` |
| `raise` | `throw` |
| `try/except` | `try/catch` |
| `None` | `null` |
| `str` | `string` |
| JSON処理 | `System.Text.Json` |
| HTTPリクエスト | `HttpClient` |
| ロギング | カスタム`Logger`クラス |

## テストの実行

```bash
# テストの実行
dotnet test

# カバレッジ付きテスト実行
dotnet test /p:CollectCoverage=true
```

## ビルドと発行

```bash
# ビルド
dotnet build

# デバッグビルド
dotnet build -c Debug

# リリースビルド
dotnet build -c Release

# NuGetパッケージ作成
dotnet pack -c Release
```

## 今後の拡張予定

1. **ファイルアップロード機能の改善**
   - 実際のGoogle Cloud Storageへのアップロード実装
   - 進捗報告機能

2. **より詳細なレスポンス解析**
   - 複数の画像型の完全サポート
   - メタデータ管理の拡張

3. **キャッシング機能**
   - Gem情報のキャッシング
   - レスポンスキャッシュ

4. **Retry ロジック**
   - 指数バックオフ
   - カスタマイズ可能な再試行ポリシー

5. **ロギング拡張**
   - ファイルロギング
   - 構造化ログサポート

6. **WebSocket サポート**
   - より効率的なストリーミング
   - リアルタイム接続

## パフォーマンス最適化

1. `HttpClient` の再利用（シングルトンパターン）
2. ストリーミング時のメモリ効率
3. JSON シリアライゼーションの最適化
4. 非同期操作の活用

## セキュリティ考慮事項

1. クッキーの安全な保存
2. HTTPS 通信の強制
3. 入力値のバリデーション
4. CSRF 対策

## トラブルシューティング

### 認証エラー
- クッキーが正しく設定されているか確認
- クッキーが期限切れていないか確認

### タイムアウト エラー
- タイムアウト値を増やす
- ネットワーク接続を確認

### レート制限
- リクエスト間隔を空ける
- バッチ処理を活用

## 参考資料

- [元の Python プロジェクト](https://github.com/HanaokaYuzu/Gemini-API)
- [Google Gemini](https://gemini.google.com/)
- [.NET ドキュメント](https://docs.microsoft.com/dotnet/)
- [C# 言語リファレンス](https://docs.microsoft.com/en-us/dotnet/csharp/)
