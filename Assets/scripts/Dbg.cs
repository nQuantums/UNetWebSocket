using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// サーバーとクライアントで同じデバッグ用変数を操作できるようにするコンポーネント
/// <para><see cref="Instance"/>で実体を取得できる。</para>
/// <para><see cref="OnDestroy"/>で実体のリファレンスがクリアされる。</para>
/// </summary>
public class Dbg : NetworkBehaviour {
	static Dbg _Instance;

	/// <summary>
	/// シーン上に１つ存在することにしてる実体の取得
	/// <para>シーンに配置したものは最初 inactive なので<see cref="Server"/>の子として配置しておき、<see cref="Server.SpawnAllNetworkIdentityChildren"/>呼出し後にアクセス可能になる</para>
	/// </summary>
	public static Dbg Instance {
		get {
			if (_Instance == null) {
				var obj = GameObject.FindObjectOfType<Dbg>();
				if (obj != null)
					_Instance = obj.GetComponent<Dbg>();
			}
			return _Instance;
		}
	}

	/// <summary>
	/// 実体破棄時の処理
	/// </summary>
	protected virtual void OnDestroy() {
		_Instance = null;
	}


	//[SyncVar]
	//public float MissileForce;
	//[SyncVar]
	//public Vector2 MissileForceAt;
}
