using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// このアプリ用のネットワークメッセージID一覧
/// </summary>
public static class MyMsgType {
	/// <summary>
	/// ギミック無効化要求
	/// </summary>
	public const short DisableGimmick = MsgType.Highest + 1;

	/// <summary>
	/// ギミック有効化要求
	/// </summary>
	public const short EnableGimmick = MsgType.Highest + 2;

	/// <summary>
	/// ギミック衝突判定有効化要求
	/// </summary>
	public const short EnableGimmickCollision = MsgType.Highest + 3;
}

/// <summary>
/// ギミックリセット要求
/// </summary>
public class MyMessageBase : MessageBase {
}
