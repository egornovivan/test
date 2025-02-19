using System.Collections;
using System.Collections.Generic;
using CustomCharactor;
using UnityEngine;

namespace AppearBlendShape;

public class AppearBuilder
{
	private static List<Transform> _tmpOutBones = new List<Transform>();

	private static List<CombineInstance> _tmpCombineInstances = new List<CombineInstance>();

	private static int _idxTmpMesh = 0;

	private static List<int> _tmpWeightIdxs = new List<int>();

	private static List<float> _tmpWeightVals = new List<float>();

	private static List<Mesh> _tmpMeshs = new List<Mesh>();

	private static void ResetTmpMeshes()
	{
		_idxTmpMesh = 0;
	}

	private static Mesh PeekTmpMesh()
	{
		if (_idxTmpMesh >= _tmpMeshs.Count)
		{
			_idxTmpMesh = _tmpMeshs.Count;
			_tmpMeshs.Add(new Mesh());
		}
		return _tmpMeshs[_idxTmpMesh++];
	}

	private static Mesh BakeToMesh(CustomPartInfo partInfo, AppearData appearData)
	{
		SkinnedMeshRenderer smr = partInfo.Smr;
		if (string.IsNullOrEmpty(partInfo.ModelName) || smr.sharedMesh.blendShapeCount == 0)
		{
			return smr.sharedMesh;
		}
		EMorphItem eMorphItem = EMorphItem.MaxFoot;
		EMorphItem eMorphItem2 = EMorphItem.MaxFoot;
		if (partInfo.ModelName.Contains("head"))
		{
			eMorphItem = EMorphItem.Min;
			eMorphItem2 = EMorphItem.MaxFace;
		}
		else if (partInfo.ModelName.Contains("torso"))
		{
			eMorphItem = EMorphItem.MaxFace;
			eMorphItem2 = EMorphItem.MaxUpperbody;
		}
		else if (partInfo.ModelName.Contains("legs"))
		{
			eMorphItem = EMorphItem.MaxUpperbody;
			eMorphItem2 = EMorphItem.MaxLowerbody;
		}
		else if (partInfo.ModelName.Contains("feet"))
		{
			eMorphItem = EMorphItem.MaxHand;
			eMorphItem2 = EMorphItem.MaxFoot;
		}
		else if (partInfo.ModelName.Contains("hand"))
		{
			eMorphItem = EMorphItem.MaxLowerbody;
			eMorphItem2 = EMorphItem.MaxHand;
		}
		if (eMorphItem != eMorphItem2)
		{
			_tmpWeightIdxs.Clear();
			_tmpWeightVals.Clear();
			for (int i = (int)eMorphItem; i < (int)eMorphItem2; i++)
			{
				float num = appearData.GetWeight((EMorphItem)i) * 100f;
				int num2 = (int)(i - eMorphItem);
				if (num > 0f)
				{
					num2 *= 2;
				}
				else
				{
					num = 0f - num;
					num2 = num2 * 2 + 1;
				}
				if (smr.sharedMesh.blendShapeCount > num2)
				{
					_tmpWeightIdxs.Add(num2);
					_tmpWeightVals.Add(smr.GetBlendShapeWeight(num2));
					smr.SetBlendShapeWeight(num2, num);
				}
			}
			Mesh mesh = PeekTmpMesh();
			smr.BakeMesh(mesh);
			mesh.boneWeights = smr.sharedMesh.boneWeights;
			mesh.bindposes = smr.sharedMesh.bindposes;
			if (_tmpWeightIdxs.Count > 0)
			{
				for (int j = 0; j < _tmpWeightIdxs.Count; j++)
				{
					smr.SetBlendShapeWeight(_tmpWeightIdxs[j], _tmpWeightVals[j]);
				}
			}
			return mesh;
		}
		return smr.sharedMesh;
	}

	private static void CopyBonesByName(Transform[] srcBones, Dictionary<string, Transform> targetBones, List<Transform> outBones)
	{
		foreach (Transform transform in srcBones)
		{
			string text = transform.name.ToLower();
			if (targetBones.TryGetValue(text, out var value))
			{
				outBones.Add(value);
			}
			else
			{
				Debug.LogError("cant find bone:" + text);
			}
		}
	}

	private static void SetColor(Material pMat, AppearData appearData)
	{
		if (pMat.name.Contains("eye"))
		{
			pMat.SetColor("_SkinColor", appearData.mEyeColor);
		}
		else if (pMat.name.Contains("hair"))
		{
			pMat.SetColor("_Color", appearData.mHairColor);
		}
		else if (pMat.name.Contains("head"))
		{
			pMat.SetColor("_Color", appearData.skinColor);
		}
		else if (!pMat.name.Contains("Helmet"))
		{
			pMat.SetColor("_SkinColor", appearData.skinColor);
		}
		else if (pMat.name.Contains("Helmet_10A"))
		{
			pMat.SetFloat("_Mode", 3f);
			pMat.SetInt("_SrcBlend", 1);
			pMat.SetInt("_DstBlend", 10);
			pMat.SetInt("_ZWrite", 0);
			pMat.DisableKeyword("_ALPHATEST_ON");
			pMat.DisableKeyword("_ALPHABLEND_ON");
			pMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			pMat.renderQueue = 3000;
		}
	}

	private static void BuildMesh(List<CustomPartInfo> partInfos, Dictionary<string, Transform> targetBones, SkinnedMeshRenderer targetSmr, AppearData appearData)
	{
		targetSmr.sharedMesh.Clear(keepVertexLayout: false);
		ResetTmpMeshes();
		_tmpOutBones.Clear();
		_tmpCombineInstances.Clear();
		int count = partInfos.Count;
		for (int i = 0; i < count; i++)
		{
			CustomPartInfo customPartInfo = partInfos[i];
			Mesh mesh = BakeToMesh(customPartInfo, appearData);
			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = mesh;
				item.subMeshIndex = j;
				_tmpCombineInstances.Add(item);
				CopyBonesByName(customPartInfo.Smr.bones, targetBones, _tmpOutBones);
			}
		}
		targetSmr.sharedMesh.CombineMeshes(_tmpCombineInstances.ToArray(), mergeSubMeshes: false, useMatrices: false);
		targetSmr.bones = _tmpOutBones.ToArray();
	}

	private static void BuildMat(List<CustomPartInfo> partInfos, SkinnedMeshRenderer targetSmr, AppearData appearData)
	{
		for (int i = 0; i < targetSmr.sharedMaterials.Length; i++)
		{
			Object.Destroy(targetSmr.sharedMaterials[i]);
		}
		List<Material> list = new List<Material>();
		int count = partInfos.Count;
		for (int j = 0; j < count; j++)
		{
			CustomPartInfo customPartInfo = partInfos[j];
			for (int k = 0; k < customPartInfo.Smr.sharedMaterials.Length; k++)
			{
				Material material = Object.Instantiate(customPartInfo.Smr.sharedMaterials[k]);
				SetColor(material, appearData);
				list.Add(material);
			}
		}
		targetSmr.materials = list.ToArray();
	}

	private static void GetPartInfos(IEnumerable<string> strPartInfos, List<CustomPartInfo> outPartInfos)
	{
		List<CustomPartInfo> list = new List<CustomPartInfo>();
		foreach (string strPartInfo in strPartInfos)
		{
			if (!string.IsNullOrEmpty(strPartInfo))
			{
				CustomPartInfo customPartInfo = new CustomPartInfo(strPartInfo);
				if (customPartInfo.Smr != null)
				{
					outPartInfos.Add(customPartInfo);
				}
			}
		}
	}

	private static void GetTargetBones(GameObject rootModel, Dictionary<string, Transform> outTargetBones)
	{
		_tmpOutBones.Clear();
		rootModel.GetComponentsInChildren(includeInactive: true, _tmpOutBones);
		foreach (Transform tmpOutBone in _tmpOutBones)
		{
			string key = tmpOutBone.name.ToLower();
			if (!outTargetBones.ContainsKey(key))
			{
				outTargetBones.Add(key, tmpOutBone);
			}
		}
	}

	public static SkinnedMeshRenderer GetTargetSmr(GameObject rootModel)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = rootModel.GetComponent<SkinnedMeshRenderer>();
		if (null == skinnedMeshRenderer)
		{
			skinnedMeshRenderer = rootModel.gameObject.AddComponent<SkinnedMeshRenderer>();
		}
		if (skinnedMeshRenderer.sharedMesh == null)
		{
			skinnedMeshRenderer.sharedMesh = new Mesh();
			skinnedMeshRenderer.sharedMesh.name = "PlayerAppearance1";
		}
		return skinnedMeshRenderer;
	}

	public static IEnumerator BuildAsync(GameObject rootModel, AppearData appearData, IEnumerable<string> strPartInfos)
	{
		List<CustomPartInfo> partInfos = new List<CustomPartInfo>();
		Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();
		SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
		GetPartInfos(strPartInfos, partInfos);
		GetTargetBones(rootModel, targetBones);
		BuildMesh(partInfos, targetBones, targetSmr, appearData);
		yield return 0;
		for (int i = 0; i < targetSmr.sharedMaterials.Length; i++)
		{
			Object.Destroy(targetSmr.sharedMaterials[i]);
		}
		List<Material> materials = new List<Material>();
		int nParts = partInfos.Count;
		for (int j = 0; j < nParts; j++)
		{
			CustomPartInfo partInfo = partInfos[j];
			int nMats = partInfo.Smr.sharedMaterials.Length;
			yield return 0;
			for (int k = 0; k < nMats; k++)
			{
				Material mat = Object.Instantiate(partInfo.Smr.sharedMaterials[k]);
				SetColor(mat, appearData);
				materials.Add(mat);
				yield return 0;
			}
		}
		targetSmr.materials = materials.ToArray();
	}

	public static SkinnedMeshRenderer Build(GameObject rootModel, AppearData appearData, IEnumerable<string> strPartInfos)
	{
		List<CustomPartInfo> list = new List<CustomPartInfo>();
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
		GetPartInfos(strPartInfos, list);
		GetTargetBones(rootModel, dictionary);
		BuildMesh(list, dictionary, targetSmr, appearData);
		BuildMat(list, targetSmr, appearData);
		return targetSmr;
	}

	public static void ApplyColor(GameObject rootModel, AppearData appearData)
	{
		SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
		if (!(targetSmr == null))
		{
			for (int i = 0; i < targetSmr.sharedMaterials.Length; i++)
			{
				SetColor(targetSmr.sharedMaterials[i], appearData);
			}
		}
	}

	public static SkinnedMeshRenderer CloneSmr(GameObject rootModel, SkinnedMeshRenderer smr)
	{
		SkinnedMeshRenderer targetSmr = GetTargetSmr(rootModel);
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		GetTargetBones(rootModel, dictionary);
		targetSmr.sharedMesh = smr.sharedMesh;
		targetSmr.sharedMaterials = smr.sharedMaterials;
		_tmpOutBones.Clear();
		CopyBonesByName(smr.bones, dictionary, _tmpOutBones);
		targetSmr.bones = _tmpOutBones.ToArray();
		return targetSmr;
	}
}
