using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ミサイルの挙動
/// </summary>
public class Missile : NetworkBehaviour {
	/// <summary>
	/// 発射主のID、発射主との衝突判定を無効化するために使用する
	/// </summary>
	[SyncVar]
	NetworkInstanceId _SpawnerId;

	/// <summary>
	/// 初期角度、<see cref="NetworkServer.Spawn"/>は何故か角度が正しくクライアントに伝えてくれないから自前でやる
	/// </summary>
	[SyncVar]
	public Quaternion _InitialRotation;

	/// <summary>
	/// 自動消滅のためのカウンタ
	/// </summary>
	int _Tick;

	Rigidbody2D _Rb;


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
	/// 開始時の処理
	/// </summary>
	void Start() {
		// 加速していくのでサーバー／クライアント共に取得しておく
		_Rb = this.GetComponent<Rigidbody2D>();

		this.transform.rotation = _InitialRotation;
	}

	/// <summary>
	/// 物理演算時の処理
	/// </summary>
	void FixedUpdate() {
		_Tick++;
		if (_Tick < 200) {
			// 加速する
			var g = Global.Instance;
			var tf = this.transform;

			// 噴射口からの力
			var force1 = tf.right * g.MissileForce;
			var at1 = tf.TransformPoint(g.MissileForceAt);

			// 羽の空気抵抗を想定
			var force2 = tf.InverseTransformVector(_Rb.velocity);
			var at2 = tf.TransformPoint(Vector2.zero);
			force2.x = 0;
			force2.y = -Mathf.Sign(force2.y) * force2.y * force2.y;
			force2 = tf.TransformVector(force2);

			_Rb.AddForceAtPosition(force1, at1, ForceMode2D.Force);
			_Rb.AddForceAtPosition(force2, at2, ForceMode2D.Force);

			// クライアント側なら一定チック毎に煙放出
			if (this.isClient) {
				if (_Tick % g.MissileSmokeInterval == 0) {
					var obj = GameObject.Instantiate<GameObject>(g.SmokePrefab, at1, Quaternion.identity);
					var smoke = obj.GetComponent<Smoke>();
					smoke.Velocity = -force1 * (Time.fixedDeltaTime * g.AirKernel);
				}
			}
		} else {
			if (this.isServer) {
				// クライアント側に爆発エフェクトを生成して自分は消える
				InstantiateExplosion(this.transform.TransformPoint(Global.Instance.MissileExplosionFrom));
				GameObject.Destroy(this.gameObject);
			}
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

		// クライアント側に爆発エフェクトを生成して自分は消える
		InstantiateExplosion(this.transform.TransformPoint(Global.Instance.MissileExplosionFrom));
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

		// ポジションを設定、ベロシティなども設定
		var rb = this.GetComponent<Rigidbody2D>();
		var tf = this.transform;
		tf.position = position;
		tf.rotation = rotation;
		_InitialRotation = rotation;
		rb.velocity = velocity + (Vector2)tf.right * Global.Instance.MissileInitialSpeed;
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
