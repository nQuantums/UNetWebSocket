using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 爆発エフェクト
/// </summary>
public class Explosion : MonoBehaviour {
	int _Tick;
	float _Angle;
	float _Omega;

	/// <summary>
	/// 開始時の処理
	/// </summary>
	void Start() {
		// 爆炎を適当に回転させたい
		_Angle = Random.Range(0, 360);
		_Omega = Random.Range(-90, 90);
		this.transform.rotation = Quaternion.Euler(0, 0, _Angle);

		// 付近のオブジェクトに爆風による衝撃を加える
		var g = Global.Instance;
		var layerMask = 1 | 1 << 9;
		var force = g.ExplosionForce;
		var pos1 = this.transform.position;
		var range = g.ExplosionRange;
		var c2s = Physics2D.OverlapCircleAll(pos1, range, layerMask);
		for(int i = 0; i < c2s.Length; i++) {
			var c2 = c2s[i];

			var gimmick = c2.GetComponent<Gimmick>();
			if (gimmick != null && gimmick.Destroyable) {
				// 相手が破壊可能なギミックならば無効化する
				gimmick.gameObject.SetActive(false);
			} else {
				// 衝撃を加える
				var rb = c2.GetComponent<Rigidbody2D>();
				if (rb != null) {
					var pos2 = rb.transform.position;
					var v = pos2 - pos1;
					var dist2 = v.sqrMagnitude;
					if (dist2 != 0) {
						rb.AddForce(v * (force / dist2), ForceMode2D.Impulse);
					}
				}
			}
		}
	}

	/// <summary>
	/// 物理演算時の処理
	/// </summary>
	void FixedUpdate() {
		// 一定時間経過後自動で消滅する
		_Tick++;
		if (_Tick < Global.Instance.ExplosionLifeTime) {
			_Angle += _Omega * Time.fixedDeltaTime / _Tick;
			this.transform.rotation = Quaternion.Euler(0, 0, _Angle);
		} else {
			GameObject.Destroy(this.gameObject);
		}
	}
}
