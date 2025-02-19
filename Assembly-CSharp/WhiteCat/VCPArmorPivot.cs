using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat;

public class VCPArmorPivot : VCPart
{
	[SerializeField]
	private ArmorType _armorType;

	[SerializeField]
	private List<BoneGroup> _boneGroups;

	private bool _isMale;

	private int _showIndex;

	[SerializeField]
	[HideInInspector]
	private bool _destroyed;

	public ArmorType armorType => _armorType;

	public bool isMale
	{
		get
		{
			return _isMale;
		}
		set
		{
			if (_isMale != value)
			{
				_isMale = value;
				_boneGroups[_showIndex].femalModel.SetActive(!value);
				_boneGroups[_showIndex].maleModel.SetActive(value);
			}
		}
	}

	public int showIndex
	{
		get
		{
			return _showIndex;
		}
		set
		{
			if (_showIndex != value)
			{
				if (_isMale)
				{
					_boneGroups[_showIndex].maleModel.SetActive(value: false);
				}
				else
				{
					_boneGroups[_showIndex].femalModel.SetActive(value: false);
				}
				_showIndex = value;
				if (_isMale)
				{
					_boneGroups[_showIndex].maleModel.SetActive(value: true);
				}
				else
				{
					_boneGroups[_showIndex].femalModel.SetActive(value: true);
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!_destroyed)
		{
			for (int i = 0; i < _boneGroups.Count; i++)
			{
				_boneGroups[i].maleModel.SetActive(value: false);
				_boneGroups[i].femalModel.SetActive(value: false);
			}
			_isMale = true;
			_showIndex = 0;
			_boneGroups[0].maleModel.SetActive(value: true);
		}
	}

	public void DestroyModels()
	{
		if (!_destroyed)
		{
			for (int i = 0; i < _boneGroups.Count; i++)
			{
				Object.Destroy(_boneGroups[i].femalModel);
				Object.Destroy(_boneGroups[i].maleModel);
			}
			_destroyed = true;
		}
	}

	public Transform GetPivot(int index)
	{
		return _boneGroups[index].bonePivot;
	}
}
