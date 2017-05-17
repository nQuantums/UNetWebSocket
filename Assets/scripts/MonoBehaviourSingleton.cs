using UnityEngine;

/// <summary>
/// <see cref="MonoBehaviour"/>を継承するシングルトンの基本クラス
/// <para><see cref="Instance"/>で実体を取得できる。</para>
/// <para><see cref="OnDestroy"/>で実体のリファレンスがクリアされる。</para>
/// </summary>
public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour {
	static T _Instance;

	/// <summary>
	/// シーン上に１つ存在することにしてる実体の取得
	/// </summary>
	public static T Instance {
		get {
			if (_Instance == null) {
				var obj = GameObject.FindObjectOfType<T>();
				if (obj != null)
					_Instance = obj.GetComponent<T>();
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
}
