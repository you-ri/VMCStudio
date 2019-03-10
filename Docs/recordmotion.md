# アニメーションの収録と出力

保存形式にはGenericアニメーションと Humanoidアニメーションの２種類があり、使用目的によってどちらの形式で保存するかを選択します。

## Genericアニメーションで保存

FBX出力する場合はこちらを使います。

+ キャプチャーしたアニメーションをFBX形式で出力することができます。
+ Timeline で表情を後編集できます。

保存方法

1. VMC Studio オブジェクトの VMCGenericAnimationRecorder の Record Transfom が ON になっていることを確認します。

    ![](https://imgur.com/bXHUJ4c.png)

2. 再生中に Display2 の START RECORDING ボタンを押すとモーションの記録が開始されます。

    ![](https://imgur.com/8cgNCzk.png)

3. 再生を停止するか STOP RECORDING ボタンを押すと Recordings フォルダに アニメーションアセット (\*.anim) が出力されます。

保存したアニメーションはFBX出力や Timeline を使った映像編集に利用することができます。

## Humanoidアニメーションで保存

収録後にUnityエディターを使って映像編集する場合に最適です。

+ Timeline で表情を後編集できます。
+ Timeline で指の動きを後編集できます。

保存方法

1. VMC Studio オブジェクトの EMRHumanoidAnimationRecorder コンポーネントを有効にして、VMCGenericAnimationRecorder の Record Transform のチェックをOFFにします。

    ![](https://imgur.com/fbsjr0a.png)

2. 再生中に Display2 の START RECORDING ボタンを押すとモーションの記録が開始されます。

    ![](https://imgur.com/8cgNCzk.png)

3. 再生を停止するか STOP RECORDING ボタンを押すと Recordings フォルダに Humanoidアニメーションアセット (\*_Humanoid.anim) と表情部分のアニメーション (\*.anim) が出力されます。

保存したアニメーションは Timeline を使った映像編集に利用することができます。

## 保存したアニメーションをFBX形式でエクスポートする

1. 予め Genericアニメーションで記録、保存します。
2. Window > VMCStudio > FBXAnimationExporter を呼び出します。
   
    ![](https://imgur.com/KQEh4v0.png)

3. 出力リスト(Export Objects)の内容を確認して、Export ボタンを押します。

    ![](https://imgur.com/kv8gljp.png)

4. 最後に収録したアニメーションが Export Directory に出力されています。

再度再生すると Export Objects の内容はクリアされます。
Humanoidアニメーションは対応していません。


## 保存したアニメーションをUnityで映像編集する

1. 予め Humanoid アニメーションで記録、保存します。
2. 映像編集するためシーンを用意して、登場キャラクターと Playeble Director を追加し、 TimelineAsset を作成しておきます。
3. Timeline から Animation Track を追加。
    
    ![](https://imgur.com/1fPYOfU.png)

4. 保存した Timeline Clip を追加してプロパティから Foot IK のチェックを外す。

    ![](https://imgur.com/DzJMm3d.png)

5. 記録したアニメーションが再生されます。
