using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// グレネードの挙動
/// </summary>
public class Grenade : NetworkBehaviour {
	/// <summary>
	/// 発射主のID、発射主との衝突判定を無効化するために使用する
	/// </summary>
	[SyncVar]
	NetworkInstanceId _SpawnerId;

	/// <summary>
	/// 自動消滅のためのカウンタ
	/// </summary>
	int _Tick;


	/// <summary>
	/// クライアント側での開始時の処理
	/// </summary>
	public override void OnStartClient() {
		// 発射主とは衝突判定行わないようにする
		var spawner = ClientScene.FindLocalObject(_SpawnerId);
		if (spawner != null)
			IgnoreCollision(spawner.GetComponent<Collider2D>());
	}

	/// <summary>
	/// 物理演算時の処理
	/// </summary>
	void FixedUpdate() {
		if (!this.isServer)
			return;

		// 一定時間経過後にクライアント側に爆発エフェクトを生成して自分は消える
		_Tick++;
		if (200 <= _Tick) {
			InstantiateExplosion(this.transform.position);
			GameObject.Destroy(this.gameObject);
		}
	}

	/// <summary>
	/// 衝突開始時の処理
	/// </summary>
	void OnCollisionEnter2D(Collision2D collision) {
		if (!this.isServer)
			return;

		// 接触相手が破壊可能なギミックならば無効化する
		var gimmick = collision.collider.GetComponent<Gimmick>();
		if (gimmick != null && gimmick.Destroyable) {
			gimmick.gameObject.SetActive(false);
		}

		// 爆発エフェクトを生成して自分は消える
		InstantiateExplosion(this.transform.position);
		GameObject.Destroy(this.gameObject);
	}

	/// <summary>
	/// 弾の挙動初期化、発射主とは接触判定行われないように初期化される
	/// </summary>
	/// <param name="spawner">発射主</param>
	/// <param name="spawnerNetId">発射主のネットワークインスタンスID</param>
	/// <param name="position">発射時の位置</param>
	/// <param name="rotation">発射時の角度</param>
	/// <param name="velocity">発射時の速度</param>
	/// <param name="angularVelocity">発射時の角速度</param>
	public void Initialize(MonoBehaviour spawner, NetworkInstanceId spawnerNetId, Vector3 position, Quaternion rotation, Vector2 velocity, float angularVelocity) {
		// 発射主のネットワークID取得しておく
		_SpawnerId = spawnerNetId;

		// 進行方向計算
		var v = velocity;
		var vlen = v.magnitude;
		if (vlen != 0)
			v *= 1 / vlen;
		else
			v = new Vector2(0, 1);

		// 進行方向のちょっと先にポジションを設定、ベロシティなども設定
		var rb = this.GetComponent<Rigidbody2D>();
		this.transform.position = position + (Vector3)(v * 1.1f);
		this.transform.rotation = rotation;
		rb.velocity = velocity * 2;
		rb.angularVelocity = angularVelocity;

		// 発射主とは衝突判定行わない
		IgnoreCollision(spawner.GetComponent<Collider2D>());
	}

	/// <summary>
	/// 指定コンポーネントとのオブジェクトとは接触判定しないようにする
	/// </summary>
	void IgnoreCollision(Collider2D collider) {
		if (collider != null)
			Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collider);
	}

	/// <summary>
	/// サーバーとクライアント両方に爆発エフェクトを生成
	/// </summary>
	void InstantiateExplosion(Vector3 position) {
		GameObject.Instantiate<GameObject>(Global.Instance.ExplosionPrefab, position, Quaternion.identity);
		RpcInstantiateExplosion(position);
	}

	/// <summary>
	/// クライアント側に爆発エフェクトを生成
	/// </summary>
	[ClientRpc]
	void RpcInstantiateExplosion(Vector3 position) {
		GameObject.Instantiate<GameObject>(Global.Instance.ExplosionPrefab, position, Quaternion.identity);
	}
}
