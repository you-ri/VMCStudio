# 操作方法


## クイックスタート

1. Unity を起動します。
2. VIVE 及び VIVE Controller, VIVE Tracker の電源を入れます。
3. VMCStudio/Exsample/VMCStudio.unity を開きます。
4. Game Window を追加して、Display2 に設定します。
   
    ![](https://imgur.com/bjMz8yq.png)

5. 再生します。
6. トラッカーを振ってDisplay2に表示しているトラッカーリストから赤く光ったシリアル番号を確認し、トラック部位を確認します。
    Model Controller オブジェクトにある VMCFullBodyController の Trackers リストからシリアルに対応する部位(Tracking Point)を設定します。

    ![](https://imgur.com/EvGXFcv.png)

7. **再生中** に VMCFullBodyController コンポーネントのパラメータをコピー(Copy Component)します。 

    ![](https://imgur.com/j43QZmS.png)

8. 停止して、VMCFullBodyController コンポーネントにペースト(Past Component Values)して、トラッカーリストを更新します。
   
    ![](https://imgur.com/l48Ga5r.png)
    ![](https://imgur.com/zXRGRJI.png)

9.  実行します。
10. HMD, Controller, Tracker を装着します。
11. Display2 の Calibration ボタンを押します。

    ![](https://imgur.com/JiZARGq.png)
    
12. カウントダウンが始まります。アクターは０になるまでTポーズの姿勢で待機します。
13. キャラクターが動きます。

## オブジェクトの役割について

![](https://imgur.com/qqgsOkD.png)

### VMC Studio

VMC Studio を機能させるために必要なオブジェクトです。
全体を管理をします。各種環境設定やアニメーションレコーダーもここで制御します。

### Model Controller

モデルを動かすために必要な機能を集約しています。コントローラーやトラッカーから情報を受け取り、キャラクターを操作します。操作周りの設定をします。

### Virtual Cameras

内部に複数の仮想カメラを配置します。 Cinemachine を使ってコントロールします。


## Control Monitor 画面の説明

GameWindowの Display2 に操作ボタンやトラッカー情報等をまとめています。

![](https://imgur.com/bjMz8yq.png)

### Calibration

トラッカーをキャリブレーションする際に押します。
基本最初に１回だけ押します。

### Recenter

現在の姿勢を中心及び正面としてリセットします。

### START RECORDING

モーションの記録を開始します。


### Cameras

Display2 のカメラのイメージをクリックするとカメラを切り替えることができます。
また、 キーボードの `1` `2` `3` のキーでも可能です。

|  | |
| -- | -- |
| `1` | 固定カメラ |
| `2` | バストアップ |
| `3` | フェイスアップ |


### Backgrounds

Display2 の　Backgounrds のボタンで背景モデルや背景色を切り替えることができます。

|  | |
| -- | -- |
| `Model` | ステージモデル |
| `White` | 白背景 |
| `Green` | 緑背景 |
| `Blue` | 青背景 |


### Trackers 

認識しているデバイスのシリアルリストを表示します。

表示されるデバイスには HMD, VIVE Controller, VIVE Tracker, Game Pad 等があります。

トラッカーやコントローラーを振ると対応するバッチが赤色に光ります。シリアル番号を確認することができます。


## オリジナルVRMキャラクターの配置

1. VRMファイルをインポートします。
2. VMCStudio シーンを開きます。
3. 1 で生成したプレファブをシーンに配置します。
4. Model Controller の Target に配置したキャラクターをドラッグして設定します。


## 表情をコントローラーから操作する(開発中)

1. キャラクターに VMCBlendShapeProxy を追加します。
2. Model Controller の VMCFaceController ブレンドシェイププリセット名に対応するコントローラー入力名を割り当てます。


## 手指をコントローラーから操作する(開発中)

1. キャラクターに HandController を２つ追加します。
2. 一方の Hand Type は Right Hnadに、もう片方は Left Hand に設定します。
3. Model Controller の VoxHandsController でハンドプリセット名に対応するコントローラ入力名を割り当てます。


## ネットワーク映像送信

ネットワーク上にある [NDI] 対応ソフトで Display1 の映像を受信できます。
[OBS Studio] 等で映像受信して合成、配信、録画することを想定しています。
解像度は Display1 の解像度設定が反映されます。

![](https://imgur.com/Sk6WW5o.png)

1. VMC Studio オブジェクトの `Enable NDI` のチェックをONにします。
2. 再生します。
3. NDI 対応ソフトで映像の受信を確認します。
