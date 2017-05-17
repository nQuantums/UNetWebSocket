using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// サーバー側の管理処理を行う、シーンに配置した<see cref="NetworkIdentity"/>コンポーネントを持つオブジェクトのルート
/// <para>シーンに配置した<see cref="NetworkIdentity"/>コンポーネントを持つオブジェクトは勝手に inactive になってるので、サーバー側初期化時に自前で activate して<see cref="NetworkServer.Spawn"/>を呼び出す必要がある。</para>
/// </summary>
public class Server : MonoBehaviourSingleton<Server> {
	/// <summary>
	/// 接続待ち受けポート番号
	/// </summary>
	public int Port;

	/// <summary>
	/// WebSocketサーバーにするかどうか
	/// </summary>
	public bool UseWebSockets;

	/// <summary>
	/// サーバー動作時も強制的にカメラを有効化するかどうか
	/// </summary>
	public bool ActivateCamera;

	/// <summary>
	/// 無効化して位置を初期化したギミックを有効化するためのカウントダウンカウンタ
	/// </summary>
	int _ResetEnableCountdown;


	/// <summary>
	/// 開始前の処理
	/// </summary>
	void Awake() {
		// もしサーバーとして実行中じゃなかったら何もしない
		if (!Global.Instance.IsServer)
			return;

		// バックグラウンドでも動作させておくとデバッグしやすい
		Application.runInBackground = true;

		// WebSocket サーバーとして初期化する
		NetworkServer.useWebSockets = this.UseWebSockets;

		// クライアントから接続された際のイベントハンドラ登録
		NetworkServer.RegisterHandler(
			MsgType.Connect,
			(netMsg) => {
				Debug.Log("player connected from: " + netMsg.conn.address);
			}
		);

		// クライアントから接続後のプレイヤーオブジェクト作成イベントハンドラ登録
		NetworkServer.RegisterHandler(
			MsgType.AddPlayer,
			(netMsg) => {
				// 接続にプレイヤーオブジェクトを結びつける
				NetworkServer.AddPlayerForConnection(
					netMsg.conn, // 接続
					Instantiate<GameObject>(Global.Instance.PlayerPrefab), // プレイヤーオブジェクト
					0 // 同接続内でのオブジェクトインデックス番号
				);
			}
		);

		// クライアントから切断された際のイベントハンドラ登録
		NetworkServer.RegisterHandler(
			MsgType.Disconnect,
			(netMsg) => {
				Debug.Log("player disconnected from: " + netMsg.conn.address);
				var go = netMsg.conn.playerControllers[0].gameObject;

				// 全クライアントに指定オブジェクトを破棄する通知を行う
				NetworkServer.UnSpawn(go);
				// オブジェクト破棄
				Destroy(go);
			}
		);

		// 接続待ち受け開始
		NetworkServer.Listen(this.Port);
	}

	/// <summary>
	/// 開始時の処理
	/// </summary>
	void Start() {
		// もしサーバーとして実行中じゃなかったら何もしない
		if (!Global.Instance.IsServer)
			return;

		// 必要ならばカメラを有効化する
		if (this.ActivateCamera || Array.IndexOf(Environment.GetCommandLineArgs(), "-activateCamera") < 0) {
			var pc = Global.Instance.PlayerCamera;
			if (pc != null)
				pc.gameObject.SetActive(true);
		}

		// シーンに配置した NetworkIdentity コンポーネントを持つオブジェクトは勝手に inactive になってるので activate して Spawn しておく
		SpawnAllNetworkIdentityChildren();
	}

	/// <summary>
	/// 画面やらの更新処理
	/// </summary>
	void Update() {
		if (!Global.Instance.IsServer)
			return;

		// ギミック初期化要求がある状態なら処理する
		if (0 != _ResetEnableCountdown) {
			_ResetEnableCountdown--;
			if (_ResetEnableCountdown == 50) {
				// ギミックを有効化する
				var gimmicks = this.GetComponentsInChildren<Gimmick>(true);
				for (int i = 0; i < gimmicks.Length; i++)
					gimmicks[i].gameObject.SetActive(true);

				// 全クライアントにギミック有効化要求を送る
				NetworkServer.SendToAll(MyMsgType.EnableGimmick, new MyMessageBase());
			} else if (_ResetEnableCountdown == 0) {
				// 全クライアントにギミック衝突判定有効化要求を送る
				NetworkServer.SendToAll(MyMsgType.EnableGimmickCollision, new MyMessageBase());
			}
		}
	}

	/// <summary>
	/// <see cref="NetworkIdentity"/>コンポーネントを持つ子を activate して<see cref="NetworkServer.Spawn"/>呼び出す
	/// </summary>
	void SpawnAllNetworkIdentityChildren() {
		foreach (var ni in this.GetComponentsInChildren<NetworkIdentity>(true)) {
			ni.gameObject.SetActive(true);
			NetworkServer.Spawn(ni.gameObject);
		}
	}

	/// <summary>
	/// ギミックを初期化する
	/// </summary>
	public void ResetGimmicks() {
		var gimmicks = this.GetComponentsInChildren<Gimmick>(true);

		// 全クライアントにギミック無効化要求を送る
		NetworkServer.SendToAll(MyMsgType.DisableGimmick, new MyMessageBase());

		// 一旦無効化しないと飛び散ったギミックが集まる時にプレイヤーを絡めとってしまうので無効化する
		for (int i = 0; i < gimmicks.Length; i++)
			gimmicks[i].gameObject.SetActive(false);

		// 位置を初期化
		for (int i = 0; i < gimmicks.Length; i++)
			gimmicks[i].Initialize();

		// ギミック有効化するまでのカウント設定
		_ResetEnableCountdown = 100;
	}
}
