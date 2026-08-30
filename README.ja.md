# Veilay

[English](README.md)

![アリーナ上でモデルを隠し Splatoon オーバーレイを表示](docs/screenshots/veilay-overlay-730x380.png)

Veilay は、キャラクターモデルをローカルで隠し、選択した HUD を消し、カメラを固定することができる Dalamud プラグインです。主に Splatoon などの描画系プラグインの動作を、キャラクターモデル情報を隠した上で共有するための機能が含まれます。

プレイヤー／ペット／NPC／敵のモデルを非表示またはダミー置換できます。エンティティは消さず残すので、Splatoon などはそのまま位置を追えます。また、ネイティブ HUD の非表示や戦闘エフェクトのオフ、固定カメラの配置が可能です。

固定カメラ機能は Cammy のフリーカメラと衝突するので、注意が必要です。

## インストール

1. `/xlsettings` を実行し、**試験的機能**タブを開く
2. **カスタムプラグインリポジトリ** に次の URL を追加する:

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

1. `/xlplugins` を実行し、**Veilay** をインストールする



## 機能

- **Models** — Enable Veilay。Player / Pets / NPC / Enemy の非表示またはダミー置換（エンティティは残す）とネームプレート非表示
- **Native** — HUD Layout の選択非表示、リプレイ中の戦闘エフェクト／ターゲットライン／フライテキスト／ポップアップテキストオフ
- **Camera** — Center / Pos / Angle の固定（Cammy FreeCam とは併用注意）



## コマンド


| コマンド      | 説明                |
| --------- | ----------------- |
| `/veilay` | Veilay ウィンドウの表示切替 |




## 開発者向け

1. ビルド: `dotnet build Veilay.sln -c Release -p:Platform=x64`
2. Dalamud の **dev plugin** パスを `Veilay/bin/Release/` に向ける
3. プラグインインストーラ（dev）で **Veilay** を有効化



## ライセンス

[AGPL-3.0-or-later](LICENSE)