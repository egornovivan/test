using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPPointInitiative : SPPoint
{
	public int count;

	public float interval;

	public int[] pathIDs;

	private List<GameObject> objs = new List<GameObject>();

	private List<AssetReq> reqList = new List<AssetReq>();

	public List<GameObject> objects => objs;

	public override void ClearDeathEvent()
	{
		base.ClearDeathEvent();
		foreach (GameObject obj in objs)
		{
			if (obj != null)
			{
				AiObject component = obj.GetComponent<AiObject>();
				if (component != null)
				{
					component.DeathHandlerEvent -= OnDeath;
				}
			}
		}
	}

	private void Start()
	{
		Init(IntVector4.Zero);
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		foreach (AssetReq req in reqList)
		{
			if (req != null)
			{
				req.ReqFinishHandler -= OnSpawned;
			}
		}
	}

	public void ActivateInitiative(bool value)
	{
		if (value)
		{
			StartCoroutine(SpawnAI());
		}
		else
		{
			StopAllCoroutines();
		}
	}

	private bool IsRevisePosition()
	{
		float num = 32f;
		if (AiUtil.CheckPositionOnGround(base.position, out var hitInfo, num, num, AiUtil.groundedLayer))
		{
			base.position = hitInfo.point;
			return true;
		}
		return false;
	}

	private void OnAiDestroy(AiObject aiObj)
	{
		if (aiObj != null && objs.Contains(aiObj.gameObject))
		{
			objs.Remove(aiObj.gameObject);
		}
	}

	protected override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		if (!objs.Contains(obj))
		{
			objs.Add(obj);
		}
		AiObject component = obj.GetComponent<AiObject>();
		if (component != null)
		{
			component.DestroyHandlerEvent += OnAiDestroy;
		}
	}

	private IEnumerator SpawnAI()
	{
		while (count > 0)
		{
			yield return new WaitForSeconds(interval);
			if (base.active && IsRevisePosition())
			{
				int id = pathIDs[Random.Range(0, pathIDs.Length)];
				AssetReq req = AIResource.Instantiate(id, base.position, Quaternion.identity, OnSpawned);
				reqList.Add(req);
				count--;
			}
		}
	}
}
