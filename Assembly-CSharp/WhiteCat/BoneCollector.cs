using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat;

public class BoneCollector : MonoBehaviour
{
	[Serializable]
	public struct BoneGroup
	{
		public string name;

		public Transform model;

		public Transform ragdoll;
	}

	private struct EquipGroup
	{
		public int bone;

		public Transform equip;
	}

	[SerializeField]
	private List<BoneGroup> _boneGroups;

	private List<EquipGroup> _equipGroup = new List<EquipGroup>(4);

	private bool _isRagdoll;

	public bool isRagdoll
	{
		get
		{
			return _isRagdoll;
		}
		set
		{
			if (value == _isRagdoll)
			{
				return;
			}
			_isRagdoll = value;
			for (int i = 0; i < _equipGroup.Count; i++)
			{
				EquipGroup equipGroup = _equipGroup[i];
				if ((bool)equipGroup.equip)
				{
					if (_isRagdoll)
					{
						SetRagdollParent(equipGroup.equip, _boneGroups[equipGroup.bone].ragdoll);
					}
					else
					{
						SetModelParent(equipGroup.equip, _boneGroups[equipGroup.bone].model);
					}
				}
			}
		}
	}

	private int FindBoneGroup(string boneName)
	{
		for (int i = 0; i < _boneGroups.Count; i++)
		{
			if (_boneGroups[i].name == boneName)
			{
				return i;
			}
		}
		return -1;
	}

	private void SetModelParent(Transform equipment, Transform parent)
	{
		equipment.SetParent(parent, worldPositionStays: false);
	}

	private void SetRagdollParent(Transform equipment, Transform parent)
	{
		equipment.SetParent(parent, worldPositionStays: false);
	}

	public int FindEquipGroup(Transform equipment)
	{
		for (int i = 0; i < _equipGroup.Count; i++)
		{
			if (_equipGroup[i].equip == equipment)
			{
				return i;
			}
		}
		return -1;
	}

	public Transform FindModelBone(string boneName)
	{
		return _boneGroups[FindBoneGroup(boneName)].model;
	}

	public void AddEquipment(Transform equipment, string boneName)
	{
		int num = FindBoneGroup(boneName);
		if (num >= 0)
		{
			if (isRagdoll)
			{
				SetRagdollParent(equipment, _boneGroups[num].ragdoll);
			}
			else
			{
				SetModelParent(equipment, _boneGroups[num].model);
			}
			EquipGroup item = default(EquipGroup);
			item.bone = num;
			item.equip = equipment;
			_equipGroup.Add(item);
		}
	}

	public bool RemoveEquipment(Transform equipment)
	{
		int num = FindEquipGroup(equipment);
		if (num >= 0)
		{
			_equipGroup.RemoveAt(num);
			equipment.SetParent(null, worldPositionStays: false);
			return true;
		}
		return false;
	}

	public void SwitchBone(Transform equipment, string boneName)
	{
		if (RemoveEquipment(equipment))
		{
			AddEquipment(equipment, boneName);
		}
	}
}
