using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class NpcHatreTargets : MonoBehaviour
{
	private static NpcHatreTargets mInstance;

	public List<PeEntity> mHatredTargets;

	private List<PeEntity> tempList;

	public static NpcHatreTargets Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		mHatredTargets = new List<PeEntity>();
		tempList = new List<PeEntity>();
	}

	private void Start()
	{
		StartCoroutine(ReflashTargets(10f));
	}

	private void Update()
	{
	}

	private IEnumerator ReflashTargets(float time)
	{
		while (true)
		{
			if (mHatredTargets.Count > 0)
			{
				mHatredTargets.Clear();
			}
			yield return new WaitForSeconds(time);
		}
	}

	private bool ContainTarget(PeEntity target)
	{
		return mHatredTargets.Contains(target);
	}

	private void AddTarget(PeEntity target)
	{
		if (!ContainTarget(target))
		{
			mHatredTargets.Add(target);
		}
	}

	private bool RemoveTarget(PeEntity target)
	{
		return mHatredTargets.Remove(target);
	}

	public void TryAddInTarget(PeEntity SelfEntity, PeEntity TargetEntity, float damage, bool trans = false)
	{
		if (TargetEntity == null)
		{
			return;
		}
		if (!trans && !ContainTarget(TargetEntity))
		{
			AddTarget(TargetEntity);
		}
		float radius = ((!TargetEntity.IsBoss && SelfEntity.proto != EEntityProto.Doodad) ? 64f : 128f);
		int num = (int)SelfEntity.GetAttribute(AttribType.DefaultPlayerID);
		bool flag = false;
		if (GameConfig.IsMultiClient && 1 == 0)
		{
			return;
		}
		List<PeEntity> list = null;
		list = ((num == 5 || num == 6) ? PeSingleton<EntityMgr>.Instance.GetEntitiesFriendly(SelfEntity.peTrans.position, radius, num, SelfEntity.ProtoID, isDeath: false, SelfEntity) : PeSingleton<EntityMgr>.Instance.GetEntities(SelfEntity.peTrans.position, radius, num, isDeath: false, SelfEntity));
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].Equals(SelfEntity) && list[i].target != null)
			{
				list[i].target.TransferHatred(TargetEntity, damage);
			}
		}
	}

	public void OnEnemyLost(PeEntity TargetEntity)
	{
		RemoveTarget(TargetEntity);
	}
}
