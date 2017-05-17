using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ディスク状のメッシュと衝突判定を持たせるためのコンポーネント
/// </summary>
[AddComponentMenu(""), ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class Disc : MonoBehaviour {
	[SerializeField]
	float _Radius = 1f;
	[SerializeField]
	float _Thickness = 0.1f;
	[SerializeField]
	int _Division = 30;


	/// <summary>
	/// 半径
	/// </summary>
	public float Radius {
		get {
			return _Radius;
		}
		set {
			if (_Radius == value)
				return;
			_Radius = value;
			UpdateMeshAndCollider();
		}
	}

	/// <summary>
	/// ディスクの幅
	/// </summary>
	public float Thickness {
		get {
			return _Thickness;
		}
		set {
			if (_Thickness == value)
				return;
			_Thickness = value;
			UpdateMeshAndCollider();
		}
	}

	/// <summary>
	/// 一周の分割数
	/// </summary>
	public int Division {
		get {
			return _Division;
		}
		set {
			if (_Division == value)
				return;
			_Division = value;
			UpdateMeshAndCollider();
		}
	}


	/// <summary>
	/// メッシュとコライダーを更新する
	/// </summary>
	void UpdateMeshAndCollider() {
		var n = this.Division;
		var radius = this.Radius;
		var thickness = this.Thickness;
		var hole = radius != thickness;
		var pointsOuter = new Vector2[n];
		var pointsInner = new Vector2[hole ? n : 1];
		var radiusOuter = radius;
		var radiusInner = radius - this.Thickness;

		// 2次元座標でディスクを作成
		for (int i = 0; i < n; i++) {
			var rad = 2 * i * Mathf.PI / n;
			var c = Mathf.Cos(rad);
			var s = Mathf.Sin(rad);
			pointsOuter[i] = new Vector2(c * radiusOuter, s * radiusOuter);
			if (hole)
				pointsInner[i] = new Vector2(c * radiusInner, s * radiusInner);
		}
		if (!hole)
			pointsInner[0] = new Vector2(0, 0);

		// メッシュの頂点とインデックス配列を作成
		var mesh = new Mesh();
		var vertices = new Vector3[pointsOuter.Length + pointsInner.Length];
		var indices = new int[hole ? n * 3 * 2 : n * 3];
		if (hole) {
			for (int i = 0; i < n; i++) {
				vertices[i] = pointsInner[i];
				vertices[n + i] = pointsOuter[i];

				var index = i * 6;
				var j = (i + 1) % n;
				indices[index + 0] = i;
				indices[index + 1] = j;
				indices[index + 2] = j + n;
				indices[index + 3] = j + n;
				indices[index + 4] = i + n;
				indices[index + 5] = i;
			}
		} else {
			vertices[n] = pointsInner[0];
			for (int i = 0; i < n; i++) {
				vertices[i] = pointsOuter[i];

				var index = i * 3;
				indices[index + 0] = n;
				indices[index + 1] = (i + 1) % n;
				indices[index + 2] = i;
			}
		}
		mesh.vertices = vertices;
		mesh.subMeshCount = 1;
		mesh.SetTriangles(indices, 0);

		// メッシュ設定
		var mf = this.GetComponent<MeshFilter>();
		mf.sharedMesh = mesh;
		mf.sharedMesh.name = this.transform.name + "_Mesh";
		mesh.RecalculateNormals();

		var mr = this.GetComponent<MeshRenderer>();
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.receiveShadows = false;

		// ポリゴンコライダー設定
		var pc2d = this.GetComponent<PolygonCollider2D>();
		if (pc2d != null) {
			if (hole) {
				pc2d.pathCount = 2;
				pc2d.SetPath(0, pointsInner);
				pc2d.SetPath(1, pointsOuter);
			} else {
				pc2d.pathCount = 1;
				pc2d.SetPath(0, pointsOuter);
			}
		}
	}

	/// <summary>
	/// コンポーネント有効化時の処理
	/// </summary>
	/// <remarks>専用エディタ作るの面倒なのでコンポーネント有効化状態切り替わったら更新する目的。</remarks>
	void OnEnable() {
		UpdateMeshAndCollider();
	}
}
