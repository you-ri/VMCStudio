# VMC Studio

## VTuber配信及び映像編集ツール
- Unityエディターを直接操作して運用します。
- VIVE 及び VIVETracker を使います。（Oculus Rift は動作未検証）
- ３点から１０点までのトラッキングでキャラクターを動かすことができます。
- VIVE コントローラーやゲームパッドで表情や指の動きを操作できます。
- モーションキャプチャーしたアニメーションを記録することができます。その後、Unityで映像編集したり、アニメーションデーターとして外部DCCツールへ出力できます。
- マテリアルや背景、ポストエフェクト等も自由に設定できます。
- [VRM] 形式のアバターの使用を推奨します。
- ネットワーク上のデバイスに映像を送信できます。


![](https://imgur.com/7e25AUK.png)

## 必須環境
- Unity 2018.3.7 or later
- VR Ready PC
- HTC VIVE

## 準備

クローンまたはダウンロードした後、以下のアセットをインポートします。

- [Final IK 1.8](https://assetstore.unity.com/packages/tools/animation/final-ik-14290) *AssetStoreから購入する必要があります。
- [SteamVR Plugin v2.2.0](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)
- [Oculus Lipsync Unity 1.30.0](https://developer.oculus.com/downloads/package/oculus-lipsync-unity/)

Project Settings の Script Execution Order で Final IK 関連のコンポーネントを以下のように設定します。

![](https://imgur.com/IP16UCK.png)

インポート後 FBXAnimationExporterWindow でスクリプトエラーが発生して動かない場合、Package Manager から FBX Exporter を一旦削除して、再インストールすると通ります。
FBX Exporter 2.0.1-preview.5 にて動作確認しています。

## 謝辞
- [VirtualMotionCapture] に手を加える形で開発しました。
- [VTuber Hackathon 2018 岡山大会](http://www.creatorsprime.co.jp/vth2018/web.html) チーム Kenzoku 継続プロジェクトの一環として開発しました。

## ライセンス

[MIT](LICENSE)

Copyright (c) 2018 sh-akira for [VirtualMotionCapture](https://github.com/sh-akira/VirtualMotionCapture)

"Assets/External" Folders License below to their licenses.

- [EasyMotionRecorder] : Assets/External/EasyMotionRecorder
- [UniVRM](https://github.com/dwango/UniVRM) : Assets/External/VRM
- [VoxHands] : Assets/External/VoxHands
- [KlakNDI] : Assets/External/Klak/NDI


[NDI]: http://ndi.newtek.com/

[VirtualMotionCapture]: https://sh-akira.github.io/VirtualMotionCapture/

[VRM]: https://dwango.github.io/vrm/

[FBXExporter]: https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/index.html

[EasyMotionRecorder]: https://github.com/duo-inc/EasyMotionRecorder

[UnityRecorder]: https://assetstore.unity.com/packages/essentials/unity-recorder-94079

[VoxHands]: https://github.com/hiroki-o/VoxHands

[KlakNDI]: https://github.com/keijiro/KlakNDI

[OBS Studio]: https://obsproject.com/ja
