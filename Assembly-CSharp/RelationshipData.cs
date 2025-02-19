using System.Collections.Generic;
using System.IO;
using Pathea;

public class RelationshipData
{
	private const int VERSION0 = 0;

	private const int CUR_VERSION = 0;

	public int _playerId;

	public int _mountsProtoId;

	public List<MountMonsterData> mMountsDataList = new List<MountMonsterData>();

	public void AddData(PeEntity mounts)
	{
		MountMonsterData mountMonsterData = new MountMonsterData();
		mountMonsterData._monster = mounts;
		mountMonsterData._curPostion = mounts.peTrans.position;
		mountMonsterData._rotation = mounts.peTrans.rotation;
		mountMonsterData._scale = mounts.peTrans.scale;
		mountMonsterData._hp = mounts.HPPercent;
		mountMonsterData._protoId = mounts.ProtoID;
		mountMonsterData._mountsForce = mounts.monstermountCtrl.m_MountsForceDb.Copy();
		mountMonsterData._mountsSkill = mounts.monstermountCtrl.m_SkillData.CopyTo();
		mMountsDataList.Add(mountMonsterData);
	}

	public void RefleshRelationData()
	{
		for (int i = 0; i < mMountsDataList.Count; i++)
		{
			mMountsDataList[i].RefreshData();
		}
	}

	public void Clear()
	{
		mMountsDataList.Clear();
	}

	public void Import(BinaryReader r)
	{
		Clear();
		if (mMountsDataList == null)
		{
			mMountsDataList = new List<MountMonsterData>();
		}
		int num = r.ReadInt32();
		if (num >= 0)
		{
			_playerId = r.ReadInt32();
			_mountsProtoId = r.ReadInt32();
			int num2 = r.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				MountMonsterData mountMonsterData = new MountMonsterData();
				mountMonsterData.Import(r);
				mMountsDataList.Add(mountMonsterData);
			}
		}
	}

	public void Export(BinaryWriter w)
	{
		RefleshRelationData();
		w.Write(0);
		w.Write(_playerId);
		w.Write(_mountsProtoId);
		w.Write(mMountsDataList.Count);
		for (int i = 0; i < mMountsDataList.Count; i++)
		{
			mMountsDataList[i].Export(w);
		}
	}

	public bool RecoverRelationship()
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(_playerId);
		if (peEntity == null)
		{
			return false;
		}
		for (int i = 0; i < mMountsDataList.Count; i++)
		{
			if (mMountsDataList[i] != null && mMountsDataList[i]._monster != null && mMountsDataList[i]._monster.biologyViewCmpt != null && mMountsDataList[i]._monster.biologyViewCmpt.biologyViewRoot != null && mMountsDataList[i]._monster.biologyViewCmpt.biologyViewRoot.modelController != null)
			{
				MousePickRides component = mMountsDataList[i]._monster.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
				if ((bool)component)
				{
					mMountsDataList[i]._monster.monstermountCtrl.LoadCtrl(peEntity, component);
					return true;
				}
			}
		}
		return false;
	}
}
