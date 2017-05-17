using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ギミックリセットのスイッチ処理
/// </summary>
public class ResetSwitch : NetworkBehaviour {
	/// <summary>
	/// 衝突開始時の処理
	/// </summary>
	void OnCollisionEnter2D(Collision2D collision) {
		if (!this.isServer)
			return;

		// プレイヤーと接触したらギミックを初期化する
		var player = collision.collider.GetComponent<Player>();
		if (player != null)
			Server.Instance.ResetGimmicks();
	}
}
