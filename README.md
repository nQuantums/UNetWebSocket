UNetWebSocket
===
UnityのUNET勉強がてら作ったテスト用プログラム。

## 説明
UNETを構成するクラスが多くていまいちわからなかったので勉強のために作成しました。  
- メモ代わりのプログラムのため無駄にコメントを記述してあります。
- 勉強のため敢えて[NetworkManager](https://docs.unity3d.com/ScriptReference/Networking.NetworkManager.html)を使用していません。サーバー側とクライアント側でクラスを分けています。
- ネットワーク越しの物理挙動の同期の確認を目的にしています。
- 装甲車（？）を操作し、ミサイル＆グレネードを発射して挙動を確認できます。

## デモ画面
[テスト用サイト](http://www.nquantums.net/webgl/) ※サーバーいつ止めるかわかりません

![](./doc/fig1.png)

## 要求環境
- Unity5.6以上

## 今回使ったUNETクラス一覧＆概略
- [NetworkInstanceId](https://docs.unity3d.com/ScriptReference/Networking.NetworkInstanceId.html)
	- サーバー＆クライアント上で同じ要素を認識するためのID。
	- [ClientScene](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.html).[FindLocalObject](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.FindLocalObject.html) に渡すとオブジェクト取得できる。今回のプログラムでは弾の発射主と接触判定行わないようにするために使用。
- [NetworkIdentity](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity.html) コンポーネント
	- [NetworkInstanceId](https://docs.unity3d.com/ScriptReference/Networking.NetworkInstanceId.html) を保持している。
	- サーバー＆クライアント間で状態同期させたいならこのコンポーネントをセットする必要がある。
	- このコンポーネントをセットしたオブジェクトをシーンに配置すると勝手にDeactivate状態になっているため自分で[SetActive](https://docs.unity3d.com/ScriptReference/GameObject.SetActive.html)(true)を呼び出す必要がある。
- [NetworkBehaviour](https://docs.unity3d.com/ScriptReference/Networking.NetworkBehaviour.html) コンポーネント
	- サーバー＆クライアント間で状態同期やら処理する自作クラスはこれを継承する。
	- [SyncVar](https://docs.unity3d.com/ScriptReference/Networking.SyncVarAttribute.html) 属性を付与したフィールドを持たせたいなら継承する必要がある。ちなみにこの属性が付与されたフィールドはサーバー＆クライアント間で同期される。
- [NetworkTransform](https://docs.unity3d.com/ScriptReference/Networking.NetworkTransform.html) コンポーネント
	- 位置と角度を同期させる。
	- [NetworkIdentity](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity.html).[localPlayerAuthority](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity-localPlayerAuthority.html) の値により、クライアントとサーバーで以下の様に挙動が異なる。
		- クライアント側の所有オブジェクト（プレイヤーなど？）の場合はクライアント側で物理挙動等を計算し、計算結果をサーバーへ転送する。
		- サーバー側の所有オブジェクトの場合はサーバー側で物理挙動等を計算しクライアントにブロードキャストされる、これは敵ユニット等のクライアント側の所有オブジェクトでない場合の共通挙動らしい。
	- [transformSyncMode](https://docs.unity3d.com/ScriptReference/Networking.NetworkTransform-transformSyncMode.html) プロパティで位置＆角度のみ同期か、Rigidbody2D/Rigidbody3Dでの物理演算も含めた同期か決定するみたい。
- [NetworkServer](https://docs.unity3d.com/ScriptReference/Networking.NetworkServer.html)
	- サーバー側での接続待ち受け＆接続管理クラス、シングルトン。
	- 使い方、今回のプログラムでは Awake() 内で以下の様にやっている・・・
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
	- 今回プログラムでの使い方・・・
		1. Start() 内で [RegisterSpawnHandler](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.RegisterSpawnHandler.html) 呼び出して [NetworkIdentity](https://docs.unity3d.com/ScriptReference/Networking.NetworkIdentity.html) コンポーネントセットされたオブジェクトの生成、破棄ハンドラを登録する。※これはオブジェクト生成時に何か処理したい場合に使うが、普通は [RegisterPrefab](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.RegisterPrefab.html) の方を使うらしい。
		2. 上記の _client.[RegisterHandler](https://docs.unity3d.com/ScriptReference/Networking.NetworkClient.RegisterHandler.html) で登録した OnClientConnected 内で [ClientScene](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.html).[Ready](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.Ready.html)(netMsg.conn) 呼び出してサーバーにクライアント側の準備ができたことを通知。
		3. [ClientScene](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.html).[AddPlayer](https://docs.unity3d.com/ScriptReference/Networking.ClientScene.AddPlayer.html)(0) を呼び出してクライアント接続毎のID登録。

## 使い方

### Linuxでのサーバー起動例

	sudo nohup ./UNetWebSocket.x86_64 -server -batchmode -nographics -logfile ulog.txt > out.log 2> err.log < /dev/null &

## ソースコード説明
- [Global](./Assets/scripts/Global.cs) クラス： プログラム全体からアクセスするようなものをまとめた。
- [Server](./Assets/scripts/Server.cs) クラス： サーバーとしての接続待ち受け処理など。
- [Client](./Assets/scripts/Client.cs) クラス： サーバーへ接続する処理＆サーバーからのメッセージ処理。
- [Player](./Assets/scripts/Player.cs) クラス： 自機の操作など。
- [Gimmick](./Assets/scripts/Gimmick.cs) クラス： ギミックの初期位置の記憶処理など。

## ライセンス
[MIT](LICENCE.txt)

## 作者
[nQuantums](https://github.com/nQuantums)
