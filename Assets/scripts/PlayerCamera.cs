using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーを追跡するカメラ
/// </summary>
public class PlayerCamera : MonoBehaviour {
	GameObject _player;
	public GameObject Player {
		get {
			return _player;
		}
		set {
			_player = value;
			_PlayerRb = value.GetComponent<Rigidbody2D>();

			var tf = this.transform;
			var mypos = tf.position;
			var ppos = value.transform.position;
			mypos = new Vector3(ppos.x, ppos.y, mypos.z);
			tf.position = mypos;
		}
	}
	public float SmoothTime;
	Vector3 _Velocity;
	Rigidbody2D _PlayerRb;
	float _TrgZ;

	/// <summary>
	/// 開始時の処理
	/// </summary>
	void Start() {
		_TrgZ = this.transform.position.z;
	}

	/// <summary>
	/// 物理演算時の処理
	/// </summary>
	void FixedUpdate() {
		var player = this.Player;

		// 目標がいないなら何もしない
		if (player == null)
			return;

		// プレイヤーの速度から向かってる方向へカメラを先回りさせ
		// カメラ座標をスムーズに目標に近づける処理を行う
		var tf = this.transform;
		var pos = tf.position;
		var posTrg = player.transform.position;

		var v = _PlayerRb.velocity;
		posTrg.x += v.x / 5.0f;
		posTrg.y += v.y / 5.0f;

		// マウスホイールでカメラ距離調整
		var scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll != 0) {
			var g = Global.Instance;
			scroll *= Mathf.Abs(pos.z);
			_TrgZ = Mathf.Clamp(_TrgZ + scroll, g.CameraDistanceMin, g.CameraDistanceMax);
		}

		pos.x = Mathf.SmoothDamp(pos.x, posTrg.x, ref _Velocity.x, this.SmoothTime);
		pos.y = Mathf.SmoothDamp(pos.y, posTrg.y, ref _Velocity.y, this.SmoothTime);
		pos.z = Mathf.SmoothDamp(pos.z, _TrgZ, ref _Velocity.z, this.SmoothTime);

		tf.position = pos;
	}
}
