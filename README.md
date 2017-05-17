UNetWebSocket
===

## Description

## UNET構成要素一覧＆概略
- [NetworkIdentity](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity.html) コンポーネント
	- サーバー＆クライアント上で同じ要素を認識するためのIDである [NetworkInstanceId](https://docs.unity3d.com/ScriptReference/Networking.NetworkInstanceId.html) を保持している。
- [NetworkTransform](https://docs.unity3d.com/ScriptReference/Networking.NetworkTransform.html) コンポーネント
	- 位置と角度を同期させる。
	- クライアントとサーバーで以下の様に挙動が異なる。
		- クライアント側の所有オブジェクト（プレイヤーなど？）の場合はクライアント側で物理挙動等を計算し、計算結果をサーバーへ転送する。
		- サーバー側の所有オブジェクトの場合はサーバー側で物理挙動等を計算しクライアントにブロードキャストされる、これは敵ユニット等のクライアント側の所有オブジェクトでない場合の共通挙動らしい。
	- transformSyncMode プロパティで位置＆角度のみ同期か、Rigidbody2D/Rigidbody3Dでの物理演算も含めた同期か決定する？
- [NetworkServer](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer.html)
	- サーバー側での接続待ち受け＆接続管理クラス、シングルトン。
	- 使い方、Awake() 内で以下の様にする・・・
		1. NetworkServer.[useWebSockets](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer-useWebSockets.html) = true によりWebSocketを使う設定にする。
		2. NetworkServer.[RegisterHandler](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer.RegisterHandler.html)(MsgType.Connect, OnConnect) を呼び出してクライアントから接続された際のイベントハンドラ登録。
		3. NetworkServer.[RegisterHandler](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer.RegisterHandler.html)(MsgType.AddPlayer, OnAddPlayer) を呼び出してプレイヤーオブジェクトを生成するハンドラ登録。
		4. NetworkServer.[RegisterHandler](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer.RegisterHandler.html)(MsgType.Disconnect, OnPlayerDisconnect) を呼び出してクライアント切断時のイベントハンドラ登録。
		5. NetworkServer.[Listen](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer.Listen.html)(port) を呼び出して接続受付開始。
- [NetworkClient](https://docs.unity3d.com/ScriptReference/Networking.NetworkClient.html) クラス
	- クライアント側からサーバーへ接続するためのクラス。
	- 使い方、とりあえず Start() 内で以下の様にする・・・
		1. _client = new [NetworkClient](https://docs.unity3d.com/ScriptReference/Networking.NetworkClient.html)() でオブジェクト作成。
		2. _client.[Connect](https://docs.unity3d.com/ScriptReference/Networking.NetworkClient.Connect.html)(address, port) でサーバーに接続。
		3. _client.[RegisterHandler](https://docs.unity3d.com/ScriptReference/Networking.NetworkClient.RegisterHandler.html)(MsgType.Connect, OnClientConnected) で接続できた際のイベントハンドラ登録。
- [ClientScene](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.html) クラス
	- クライアント側でのシーンに関する情報を保持するシングルトン。
	- 使い方・・・
		1. Start() 内で [RegisterSpawnHandler](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.RegisterSpawnHandler.html)(_networkStateEntityProtoType.assetId, OnSpawnEntity, OnDespawnEntity) 呼び出してサーバー側でのクライアント用SpawnアセットのIDを登録する。
			- _networkStateEntityProtoType : プレイヤー用プレハブに付与した [NetworkIdentity](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity.html) コンポーネント。
			- OnSpawnEntity : このイベントハンドラ内で [Instantiate](https://docs.unity3d.com/ScriptReference/Object.Instantiate.html)<[NetworkIdentity](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity.html)>(_networkStateEntityProtoType) を呼び出してプレイヤーのオブジェクト作成。
			- OnDespawnEntity : このイベントハンドラ内で [Destroy](https://docs.unity3d.com/ScriptReference/Object.Destroy.html) を呼び出してプレイヤーのオブジェクト破棄する。
			- ※上記はあらかじめプレハブが用意されてない場合に行う処理らしい。普通は [RegisterPrefab](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.RegisterPrefab.html) を使うらしい。
		2. 上記の _client.[RegisterHandler](https://docs.unity3d.com/ScriptReference/Networking.NetworkClient.RegisterHandler.html) で登録した OnClientConnected 内で [ClientScene](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.html).[Ready](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.Ready.html)(netMsg.conn) 呼び出してサーバーにクライアント側の準備ができたことを通知。
		3. [ClientScene](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.html).[AddPlayer](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.AddPlayer.html)(0) を呼び出してクライアント接続毎のID登録。



## Demo
![](./doc/fig1.jpg)

## Requirement
- [Anaconda3](https://www.continuum.io/downloads) 4.2 [Windows 64bit](https://repo.continuum.io/archive/Anaconda3-4.2.0-Windows-x86_64.exe) / 
 [32bit](https://repo.continuum.io/archive/Anaconda3-4.2.0-Windows-x86.exe)、[Linux 64bit](https://repo.continuum.io/archive/Anaconda3-4.2.0-Linux-x86_64.sh) / [32bit](https://repo.continuum.io/archive/Anaconda3-4.2.0-Linux-x86.sh)、[MacOS X](https://repo.continuum.io/archive/Anaconda3-4.2.0-MacOSX-x86_64.sh)  
- [CUDA8](https://developer.nvidia.com/cuda-downloads) ※ChainerでGPU使うなら必要
- [cuDNN5](https://developer.nvidia.com/cudnn) ※ChainerでGPU使うなら必要
- VisualStudio2015 ※CPythonやらCUDAコンパイラで必要になる
- [Chainer](https://github.com/pfnet/chainer) 1.23
- OpenCV3

## Usage

	python ./train.py CycleGAN

## Contribution

## Licence

[MIT](LICENCE.txt)

## Author

[nQuantums](https://github.com/nQuantums)
