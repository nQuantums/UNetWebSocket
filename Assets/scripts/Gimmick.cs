using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ギミック、初期位置を記憶しておき必要に応じて初期位置に戻せる
/// </summary>
public class Gimmick : NetworkBehaviour {
	/// <summary>
	/// 破壊可能かどうか
	/// </summary>
	public bool Destroyable;

	Vector3 _InitialPosition;
	Quaternion _InitialRotation;


	/// <summary>
	/// サーバー側での開始時の処理
	/// </summary>
	public override void OnStartServer() {
		base.OnStartServer();

		// 初期位置記憶
		var tf = this.transform;
		_InitialPosition = tf.position;
		_InitialRotation = tf.rotation;
	}

	/// <summary>
	/// 初期位置に戻す
	/// </summary>
	public void Initialize() {
		var tf = this.transform;
		var rb = this.GetComponent<Rigidbody2D>();
		tf.position = _InitialPosition;
		tf.rotation = _InitialRotation;
		rb.velocity = new Vector2();
		rb.angularVelocity = 0f;
	}
}
