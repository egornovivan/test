using System.IO;
using UnityEngine;

namespace Pathea;

public class RQDialogue : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public string RqAction;

	public bool hasDone;

	private object Obj;

	private Vector3 m_RqRatePos;

	public Vector3 RqRatePos
	{
		get
		{
			if (Obj != null)
			{
				if (Obj is Vector3)
				{
					return (Vector3)Obj;
				}
				if (Obj is PeTrans)
				{
					return ((PeTrans)Obj).position;
				}
			}
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null && PeSingleton<PeCreature>.Instance.mainPlayer.peTrans != null)
			{
				return PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position;
			}
			return Vector3.zero;
		}
	}

	public override EReqType Type => EReqType.Dialogue;

	public RQDialogue()
	{
	}

	public RQDialogue(params object[] objs)
	{
		RqAction = (string)objs[0];
		hasDone = false;
		if (objs.Length > 1)
		{
			if (objs[1] == null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				Obj = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
			}
			Obj = objs[1];
		}
	}

	public bool CanDoAction()
	{
		if (RqAction != string.Empty && RqAction != "0" && !hasDone)
		{
			hasDone = true;
			return true;
		}
		return false;
	}

	public bool CanEndAction()
	{
		if (RqAction != string.Empty && hasDone)
		{
			hasDone = false;
			return true;
		}
		return false;
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
	}
}
