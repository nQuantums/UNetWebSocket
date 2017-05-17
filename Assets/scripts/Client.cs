using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// クライアント側の管理処理
/// </summary>
public class Client : MonoBehaviourSingleton<Client> {
	/// <summary>
	/// 接続先サーバーポート番号
	/// </summary>
	public int Port;

	NetworkClient _Client;


	/// <summary>
	/// 開始時の処理
	/// </summary>
	void Start () {
		// もしクライアントとして実行中じゃなかったら何もしない
		if (!Global.Instance.IsClient)
			return;

		// 物理演算があるので常時処理しておきたい
		Application.runInBackground = true;

		// ネットワーク越しに座標など同期する必要があるプレハブを登録する
		foreach (var kvp in Global.Instance.NetPrefabs) {
			ClientScene.RegisterSpawnHandler(
				kvp.Key,
				(position, assetId) => {
					return GameObject.Instantiate<GameObject>(kvp.Value);
				},
				(spawned) => GameObject.Destroy(spawned)
			);
		}

		// サーバーアドレスの取得
		string address;
		if (string.IsNullOrEmpty(Application.absoluteURL)) {
			address = "localhost";
		} else {
			address = new Uri(Application.absoluteURL).GetComponents(UriComponents.Host, UriFormat.Unescaped);
		}

		// サーバーへ接続する
		_Client = new NetworkClient();
		_Client.Connect(address, this.Port);
		_Client.RegisterHandler(
			MsgType.Connect,
			(netMsg) => {
				ClientScene.Ready(netMsg.conn);
				ClientScene.AddPlayer(0);
			}
		);

		// ギミック無効化要求ハンドラ登録
		_Client.RegisterHandler(
			MyMsgType.DisableGimmick, 
			(netMsg) => {
				var gimmicks = Server.Instance.GetComponentsInChildren<Gimmick>(true);
				for (int i = 0; i < gimmicks.Length; i++) {
					var gimmick = gimmicks[i];
					var c = gimmick.GetComponent<Collider2D>();
					if (c != null)
						c.enabled = false;
					//var ntf = gimmick.GetComponent<NetworkTransform>();
					//if (ntf != null)
					//	ntf.enabled = false;
					gimmick.gameObject.SetActive(false);
					gimmick.Initialize();
				}
			}
		);

		// ギミック有効化要求ハンドラ登録
		_Client.RegisterHandler(
			MyMsgType.EnableGimmick,
			(netMsg) => {
				var gimmicks = Server.Instance.GetComponentsInChildren<Gimmick>(true);
				for (int i = 0; i < gimmicks.Length; i++) {
					gimmicks[i].gameObject.SetActive(true);
				}
			}
		);

		// ギミック衝突判定有効化要求ハンドラ登録
		_Client.RegisterHandler(
			MyMsgType.EnableGimmickCollision,
			(netMsg) => {
				var gimmicks = Server.Instance.GetComponentsInChildren<Gimmick>(true);
				for (int i = 0; i < gimmicks.Length; i++) {
					var gimmick = gimmicks[i];
					var c = gimmick.GetComponent<Collider2D>();
					if (c != null)
						c.enabled = true;
					//var ntf = gimmick.GetComponent<NetworkTransform>();
					//if (ntf != null)
					//	ntf.enabled = true;
				}
			}
		);
	}
}
