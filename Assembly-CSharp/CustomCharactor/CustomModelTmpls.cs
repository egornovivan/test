using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCharactor;

public class CustomModelTmpls
{
	private int _nTmpls;

	private Dictionary<AvatarData, CustomTmplGo> _tmplGos = new Dictionary<AvatarData, CustomTmplGo>();

	private static CustomModelTmpls s_Inst;

	public static CustomModelTmpls Instance
	{
		get
		{
			if (s_Inst == null)
			{
				s_Inst = new CustomModelTmpls();
			}
			return s_Inst;
		}
	}

	public void Init(int nTmpl)
	{
		_nTmpls = nTmpl;
		List<CustomTmplGo> list = _tmplGos.Values.Cast<CustomTmplGo>().ToList();
		_tmplGos.Clear();
		foreach (CustomTmplGo item in list)
		{
			if (item._go != null)
			{
				Object.Destroy(item._go);
			}
		}
	}

	public CustomTmplGo GetTmplGo(AvatarData avdata)
	{
		if (avdata == null)
		{
			return null;
		}
		if (!_tmplGos.TryGetValue(avdata, out var value))
		{
			value = new CustomTmplGo();
			value._go = CreateTmplGo(avdata);
			if (_tmplGos.Count > _nTmpls)
			{
				float tStamp = value._tStamp;
				AvatarData key = avdata;
				foreach (KeyValuePair<AvatarData, CustomTmplGo> tmplGo in _tmplGos)
				{
					if (tmplGo.Value._tStamp <= tStamp)
					{
						tStamp = tmplGo.Value._tStamp;
						key = tmplGo.Key;
					}
				}
				_tmplGos.Remove(key);
			}
			_tmplGos.Add(avdata, value);
		}
		value._refCnt++;
		value._tStamp = Time.time;
		return value;
	}

	public void DismissTmplGo(AvatarData avdata)
	{
		if (avdata != null && _tmplGos.TryGetValue(avdata, out var value))
		{
			value._refCnt--;
			if (value._refCnt <= 0)
			{
				_tmplGos.Remove(avdata);
			}
		}
	}

	private static GameObject CreateTmplGo(AvatarData avdata)
	{
		string baseModel = avdata._baseModel;
		GameObject gameObject = Object.Instantiate(Resources.Load(baseModel)) as GameObject;
		PEModelController componentInChildren = gameObject.GetComponentInChildren<PEModelController>();
		if (componentInChildren == null)
		{
			Debug.LogError("[CreateTmplGo]Cannot find PEModelController in baseModel:" + baseModel);
			return null;
		}
		PERagdollController componentInChildren2 = gameObject.GetComponentInChildren<PERagdollController>();
		if (componentInChildren2 == null)
		{
			Debug.LogError("[CreateTmplGo]Cannot find PERagdollController in baseModel:" + baseModel);
			return null;
		}
		Debug.LogError("[CreateTmplGo]create baseModel:" + baseModel);
		SkinnedMeshRenderer skinnedMeshRenderer = componentInChildren.GetComponent<SkinnedMeshRenderer>();
		if (null == skinnedMeshRenderer)
		{
			skinnedMeshRenderer = componentInChildren.gameObject.AddComponent<SkinnedMeshRenderer>();
		}
		if (skinnedMeshRenderer.sharedMesh == null)
		{
			skinnedMeshRenderer.sharedMesh = new Mesh();
			skinnedMeshRenderer.sharedMesh.name = "PlayerAppearance1";
		}
		else
		{
			skinnedMeshRenderer.sharedMesh.Clear();
		}
		List<Material> list = new List<Material>();
		List<Transform> list2 = new List<Transform>();
		List<Transform> allBonesList = new List<Transform>(componentInChildren.GetComponentsInChildren<Transform>(includeInactive: true));
		foreach (string avdatum in avdata)
		{
			if (string.IsNullOrEmpty(avdatum))
			{
				continue;
			}
			SkinnedMeshRenderer smr = new CustomPartInfo(avdatum).Smr;
			if (!(null == smr))
			{
				List<Transform> collection = CustomUtils.FindSmrBonesByName(allBonesList, smr);
				int subMeshCount = smr.sharedMesh.subMeshCount;
				for (int i = 0; i < subMeshCount; i++)
				{
					list2.AddRange(collection);
				}
				list.AddRange(smr.sharedMaterials);
			}
		}
		skinnedMeshRenderer.bones = list2.ToArray();
		skinnedMeshRenderer.materials = list.ToArray();
		SkinnedMeshRenderer skinnedMeshRenderer2 = componentInChildren2.GetComponent<SkinnedMeshRenderer>();
		if (null == skinnedMeshRenderer2)
		{
			skinnedMeshRenderer2 = componentInChildren2.gameObject.AddComponent<SkinnedMeshRenderer>();
		}
		skinnedMeshRenderer2.sharedMesh = skinnedMeshRenderer.sharedMesh;
		List<Transform> allBonesList2 = new List<Transform>(componentInChildren2.GetComponentsInChildren<Transform>(includeInactive: true));
		list2 = CustomUtils.FindSmrBonesByName(allBonesList2, skinnedMeshRenderer);
		skinnedMeshRenderer2.bones = list2.ToArray();
		skinnedMeshRenderer2.sharedMaterials = skinnedMeshRenderer.sharedMaterials;
		gameObject.SetActive(value: false);
		return gameObject;
	}
}
