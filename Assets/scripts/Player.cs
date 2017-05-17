using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// プレイヤーの挙動
/// </summary>
public class Player : NetworkBehaviour {
	public GameObject[] Tires;
	WheelJoint2D[] _WheelJoints;
	Rigidbody2D _Rb;


	/// <summary>
	/// 開始時の処理
	/// </summary>
	void Start() {
		if (!this.isLocalPlayer)
			return;

		// タイヤ同士の接触判定を無くす
		var myCollider = this.GetComponent<Collider2D>();
		var tires = this.Tires;
		_WheelJoints = new WheelJoint2D[tires.Length];
		for (int i = 0; i < tires.Length; i++) {
			var tire = tires[i];
			_WheelJoints[i] = tire.GetComponent<WheelJoint2D>();

			var c1 = tire.GetComponent<Collider2D>();
			Physics2D.IgnoreCollision(c1, myCollider);
			for (int j = 0; j < tires.Length; j++) {
				if (i != j)
					Physics2D.IgnoreCollision(c1, tires[j].GetComponent<Collider2D>());
			}
		}

		_Rb = this.GetComponent<Rigidbody2D>();

		// カメラのターゲットを自分にし、有効化する
		var pc = Global.Instance.PlayerCamera;
		if (pc != null) {
			pc.Player = this.gameObject;
			pc.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// 物理演算時の処理
	/// </summary>
	void FixedUpdate() {
		if (!this.isLocalPlayer)
			return;
		var g = Global.Instance;

		// A,Dキーで車輪回転
		var av = 0f;
		if (Input.GetKey(KeyCode.A)) {
			av -= 1500.0f;
		}
		if (Input.GetKey(KeyCode.D)) {
			av += 1500.0f;
		}
		var wjs = _WheelJoints;
		for (int i = 0; i < wjs.Length; i++) {
			var wj = wjs[i];
			var m = wj.motor;
			m.motorSpeed = av;
			wj.motor = m;
		}

		// マウス左ボタンが押されたら現在位置に弾を生成
		var left = Input.GetMouseButtonDown(0);
		var right = Input.GetMouseButtonDown(1);
		if (left || right) {
			// Vector3でマウス位置座標を取得する
			var mp = Input.mousePosition;
			// Z軸修正
			mp.z = -g.PlayerCamera.transform.position.z;
			// マウス座標をスクリーン座標からワールド座標に変換する
			var mwp = Camera.main.ScreenToWorldPoint(mp);

			// 自座標→マウス座標へのベクトル計算して角度やらも計算
			var tf = this.transform;
			var position = (Vector2)tf.position;
			var toMousePosition = ((Vector2)mwp - position).normalized;
			var rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(toMousePosition.y, toMousePosition.x));

			if (left) {
				CmdFireMissile(this.netId, tf.TransformPoint(g.MissileLaunchFrom), rotation, _Rb.velocity, 0);
			} else {
				CmdFireGrenade(this.netId, position, this.transform.rotation, _Rb.velocity + toMousePosition * g.GrenadeSpeed, 0);
			}
		}
	}

	/// <summary>
	/// ミサイル発射処理
	/// </summary>
	/// <param name="myNetId">自分のネットワークインスタンスID</param>
	/// <param name="position">発射時の位置</param>
	/// <param name="rotation">発射時の角度</param>
	/// <param name="velocity">発射時の速度</param>
	/// <param name="angularVelocity">発射時の角速度</param>
	[Command]
	void CmdFireMissile(NetworkInstanceId myNetId, Vector3 position, Quaternion rotation, Vector2 velocity, float angularVelocity) {
		var obj = GameObject.Instantiate<GameObject>(Global.Instance.MissilePrefab);
		var missile = obj.GetComponent<Missile>();
		missile.Initialize(this, myNetId, position, rotation, velocity, angularVelocity);
		NetworkServer.Spawn(obj);
	}

	/// <summary>
	/// 爆弾発射処理
	/// </summary>
	/// <param name="myNetId">自分のネットワークインスタンスID</param>
	/// <param name="position">発射時の位置</param>
	/// <param name="rotation">発射時の角度</param>
	/// <param name="velocity">発射時の速度</param>
	/// <param name="angularVelocity">発射時の角速度</param>
	[Command]
	void CmdFireGrenade(NetworkInstanceId myNetId, Vector3 position, Quaternion rotation, Vector2 velocity, float angularVelocity) {
		var obj = GameObject.Instantiate<GameObject>(Global.Instance.GrenadePrefab);
		var grenade = obj.GetComponent<Grenade>();
		grenade.Initialize(this, myNetId, position, rotation, velocity, angularVelocity);
		NetworkServer.Spawn(obj);
	}
}
