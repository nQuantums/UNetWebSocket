﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Global : MonoBehaviourSingleton<Global> {
	bool? _IsServer;
	bool? _IsClient;
	Dictionary<NetworkHash128, GameObject> _NetPrefabs;

	/// <summary>
	/// 現在サーバーモードで実行中かどうか
	/// </summary>
	public bool IsServer {
		get {
			if (_IsServer == null) {
				var args = Environment.GetCommandLineArgs();
				_IsServer = (this.RunInEditorAsServer && Application.isEditor)|| 0 <= Array.IndexOf(args, "-server");
			}
			return _IsServer.Value;
		}
	}

	/// <summary>
	/// 現在クライアントモードで実行中かどうか
	/// </summary>
	public bool IsClient {
		get {
			if (_IsClient == null) {
				_IsClient = !this.IsServer;
			}
			return _IsClient.Value;
		}
	}

	/// <summary>
	/// NetworkIdentity コンポーネントを持つプレハブ一覧
	/// </summary>
	public Dictionary<NetworkHash128, GameObject> NetPrefabs {
		get {
			if (_NetPrefabs == null) {
				var gos = new List<GameObject>(new GameObject[] {
					this.PlayerPrefab,
					this.GrenadePrefab,
					this.MissilePrefab,
				});
				gos.AddRange(this.OtherNetPrefabs);
				_NetPrefabs = new Dictionary<NetworkHash128, GameObject>();
				for (int i = 0; i < gos.Count; i++) {
					var go = gos[i];
					var nid = go.GetComponent<NetworkIdentity>();
					if (nid != null)
						_NetPrefabs[nid.assetId] = go;
				}
			}
			return _NetPrefabs;
		}
	}

	/// <summary>
	/// エディタ上で実行するならサーバーとして実行するかどうか
	/// </summary>
	public bool RunInEditorAsServer;

	/// <summary>
	/// プレイヤーを追尾するカメラ
	/// </summary>
	public PlayerCamera PlayerCamera;

	/// <summary>
	/// 空気抵抗による速度の変化率
	/// </summary>
	public float AirKernel;

	/// <summary>
	/// プレイヤープレハブ
	/// </summary>
	public GameObject PlayerPrefab;

	/// <summary>
	/// グレネードプレハブ
	/// </summary>
	public GameObject GrenadePrefab;

	/// <summary>
	/// ミサイルプレハブ
	/// </summary>
	public GameObject MissilePrefab;

	/// <summary>
	/// ミサイルの煙放出間隔時間（現状はチック数で指定)
	/// </summary>
	public int MissileSmokeInterval;

	/// <summary>
	/// ミサイルの爆発元位置
	/// </summary>
	public Vector2 MissileExplosionFrom;

	/// <summary>
	/// 爆発プレハブ
	/// </summary>
	public GameObject ExplosionPrefab;

	/// <summary>
	/// 爆発の存在時間（現状はチック数で指定)
	/// </summary>
	public int ExplosionLifeTime;

	/// <summary>
	/// 爆発の影響範囲
	/// </summary>
	public float ExplosionRange;

	/// <summary>
	/// 爆発の威力
	/// </summary>
	public float ExplosionForce;

	/// <summary>
	/// 煙プレハブ
	/// </summary>
	public GameObject SmokePrefab;

	/// <summary>
	/// 煙の存在時間（現状はチック数で指定)
	/// </summary>
	public int SmokeLifeTime;

	/// <summary>
	/// その他のネットワーク越しに実体化されるプレハブ
	/// </summary>
	public GameObject[] OtherNetPrefabs;

	/// <summary>
	/// ミサイル発射位置
	/// </summary>
	public Vector2 MissileLaunchFrom;

	/// <summary>
	/// ミサイルの初速
	/// </summary>
	public float MissileInitialSpeed;

	/// <summary>
	/// ミサイルの加速力
	/// </summary>
	public float MissileForce;

	/// <summary>
	/// ミサイルの噴射口の位置
	/// </summary>
	public Vector2 MissileForceAt;

	/// <summary>
	/// 爆弾投げの速さ
	/// </summary>
	public float BombSpeed;

	/// <summary>
	/// プレイヤー追尾カメラの距離最小値
	/// </summary>
	public float CameraDistanceMin;

	/// <summary>
	/// プレイヤー追尾カメラの距離最大値
	/// </summary>
	public float CameraDistanceMax;
}
