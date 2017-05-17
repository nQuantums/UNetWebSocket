using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火と煙の挙動
/// </summary>
public class Smoke : MonoBehaviour {
	int _Tick;
	float _Angle;
	float _Omega;
	public Vector3 Velocity;

	/// <summary>
	/// 開始時の処理
	/// </summary>
	void Start() {
		// 爆炎を適当に回転させたい
		_Angle = Random.Range(0, 360);
		_Omega = Random.Range(-90, 90);
		this.transform.rotation = Quaternion.Euler(0, 0, _Angle);
	}

	/// <summary>
	/// 物理演算時の処理
	/// </summary>
	void FixedUpdate() {
		// 一定時間経過後自動で消滅する
		_Tick++;
		if (_Tick < Global.Instance.SmokeLifeTime) {
			var tf = this.transform;
			_Angle += _Omega * Time.fixedDeltaTime / _Tick;
			tf.rotation = Quaternion.Euler(0, 0, _Angle);

			var pos = tf.position;
			pos += this.Velocity;
			tf.position = pos;
		} else {
			GameObject.Destroy(this.gameObject);
		}
	}
}
