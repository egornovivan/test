using System.Collections;
using Pathea;
using UnityEngine;

namespace PeMap;

public class MapRunner : MonoBehaviour
{
	private static bool updateTile;

	private PeTrans mTrans;

	public static bool HasMaskTile => updateTile;

	public static void UpdateTile(bool value)
	{
		updateTile = value;
	}

	private void Start()
	{
		StartCoroutine(Run());
	}

	private bool GetPlayerPos(out Vector3 pos)
	{
		if (mTrans == null)
		{
			PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
			if (null != mainPlayer)
			{
				mTrans = mainPlayer.GetCmpt<PeTrans>();
			}
		}
		if (null == mTrans)
		{
			pos = Vector3.zero;
			return false;
		}
		pos = mTrans.position;
		return true;
	}

	private IEnumerator Run()
	{
		while (true)
		{
			if (GetPlayerPos(out var pos))
			{
				PeSingleton<StaticPoint.Mgr>.Instance.Tick(pos);
			}
			yield return new WaitForSeconds(1f);
			if (updateTile && GetPlayerPos(out pos))
			{
				PeSingleton<MaskTile.Mgr>.Instance.Tick(pos);
			}
			yield return new WaitForSeconds(1f);
		}
	}
}
