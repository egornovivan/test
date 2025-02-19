using System.IO;
using Pathea;
using PETools;
using UnityEngine;

public class MountMonsterData
{
	private const int VERSION0 = 0;

	private const int VERSION1 = 1;

	private const int CUR_VERSION = 1;

	public PeEntity _monster;

	public ForceData _mountsForce;

	public BaseSkillData _mountsSkill;

	public float _hp;

	public Vector3 _curPostion;

	public Quaternion _rotation;

	public Vector3 _scale;

	public int _protoId;

	public ECtrlType _eCtrltype;

	public MountMonsterData()
	{
		_mountsForce = new ForceData();
		_mountsSkill = new BaseSkillData();
	}

	public void RefreshData()
	{
		if (!(_monster == null))
		{
			_curPostion = _monster.peTrans.position;
			_rotation = _monster.peTrans.rotation;
			_scale = _monster.peTrans.scale;
			_hp = _monster.HPPercent;
			_protoId = _monster.ProtoID;
			_eCtrltype = _monster.monstermountCtrl.ctrlType;
			_mountsForce = _monster.monstermountCtrl.m_MountsForceDb.Copy();
			_mountsSkill = _monster.monstermountCtrl.m_SkillData.CopyTo();
		}
	}

	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num >= 1)
		{
			_mountsForce.Import(r);
			_mountsSkill.Import(r);
			_hp = r.ReadSingle();
			_curPostion = Serialize.ReadVector3(r);
			_rotation = Serialize.ReadQuaternion(r);
			_scale = Serialize.ReadVector3(r);
			_protoId = r.ReadInt32();
			if (num >= 1)
			{
				_eCtrltype = (ECtrlType)r.ReadInt32();
			}
			_monster = PeSingleton<PeEntityCreator>.Instance.CreateMountsMonster(this);
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(1);
		_mountsForce.Export(w);
		_mountsSkill.Export(w);
		w.Write(_hp);
		Serialize.WriteVector3(w, _curPostion);
		Serialize.WriteQuaternion(w, _rotation);
		Serialize.WriteVector3(w, _scale);
		w.Write(_protoId);
		w.Write((int)_eCtrltype);
	}
}
