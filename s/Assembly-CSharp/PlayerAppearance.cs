using System.Collections.Generic;
using UnityEngine;

public class PlayerAppearance
{
	internal AppearanceData mAppearanceData;

	public AppearanceData.BodyData[] mCurrentBodyElement = new AppearanceData.BodyData[AppearanceData.mBodyCombineElement.Length];

	internal GameObject[][] mSynchBodyBone = new GameObject[AppearanceData.mBodilyElements.Length][];

	internal GameObject mPlayer;

	internal bool mEquipModel;

	private bool mBuildFaceMesh;

	public PlayerAppearance()
	{
		for (int i = 0; i < AppearanceData.mBodilyElements.Length; i++)
		{
			mSynchBodyBone[i] = new GameObject[AppearanceData.mBodilyElements[i].Length - 2];
		}
	}

	public PlayerAppearance(AppearanceData pAD)
	{
		mAppearanceData = pAD;
		for (int i = 0; i < AppearanceData.mBodilyElements.Length; i++)
		{
			mSynchBodyBone[i] = new GameObject[AppearanceData.mBodilyElements[i].Length - 2];
		}
	}

	internal void InitAppearance(GameObject pPlayer)
	{
		mPlayer = pPlayer;
		for (int i = 0; i < AppearanceData.mBodyCombineElement.Length; i++)
		{
			mAppearanceData.mBodyData[i].GetDataInfo(mAppearanceData.mCustomStyle[i], out var path, out var name);
			mCurrentBodyElement[i] = new AppearanceData.BodyData(path, name, mAppearanceData.mBodyData[i].mSlot);
		}
		for (int j = 0; j < AppearanceData.mBodilyElements.Length; j++)
		{
			for (int k = 2; k < AppearanceData.mBodilyElements[j].Length; k++)
			{
				CheckSynchBonesBody(pPlayer.transform, AppearanceData.mBodilyElements[j][k], j, k - 2);
			}
		}
	}

	internal void CheckSynchBonesBody(Transform pT, string pBoneName, int pN, int index)
	{
		if (pT.name == pBoneName)
		{
			mSynchBodyBone[pN][index] = pT.gameObject;
		}
		else if (pT.childCount > 0)
		{
			for (int i = 0; i < pT.childCount; i++)
			{
				CheckSynchBonesBody(pT.GetChild(i), pBoneName, pN, index);
			}
		}
	}

	internal void ResetBadyData()
	{
	}

	internal void ChangeAppearance()
	{
	}

	internal PlayerAppearance Clone()
	{
		return (PlayerAppearance)MemberwiseClone();
	}

	public void AddBodyElement(string mFilePath, string mName, int pSlot)
	{
		for (int i = 0; i < mCurrentBodyElement.Length; i++)
		{
			if (mCurrentBodyElement[i].mSlot == pSlot)
			{
				mCurrentBodyElement[i].mFilePath = mFilePath;
				mCurrentBodyElement[i].mName = mName;
				mCurrentBodyElement[i].mSlot = pSlot;
			}
		}
	}

	public void DeleteBodyElement(int pSlot)
	{
		for (int i = 0; i < mCurrentBodyElement.Length; i++)
		{
			if (mCurrentBodyElement[i].mSlot == pSlot)
			{
				mAppearanceData.mBodyData[i].GetDataInfo(mAppearanceData.mCustomStyle[i], out var path, out var name);
				mCurrentBodyElement[i].mFilePath = path;
				mCurrentBodyElement[i].mName = name;
				mCurrentBodyElement[i].mSlot = mAppearanceData.mBodyData[i].mSlot;
				break;
			}
		}
	}

	public void RebulidProxyMesh(GameObject pObj)
	{
		if (pObj == null)
		{
			return;
		}
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		List<Transform> list3 = new List<Transform>();
		AppearanceData.BodyData[] array = mCurrentBodyElement;
		foreach (AppearanceData.BodyData bodyData in array)
		{
			GameObject gameObject = Resources.Load(bodyData.mFilePath) as GameObject;
			if (!gameObject)
			{
				continue;
			}
			Transform transform = gameObject.transform.FindChild(bodyData.mName);
			if (!transform)
			{
				continue;
			}
			GameObject gameObject2 = Object.Instantiate(transform.gameObject);
			if (!gameObject2)
			{
				continue;
			}
			SkinnedMeshRenderer component = gameObject2.GetComponent<SkinnedMeshRenderer>();
			if (!component)
			{
				Object.Destroy(gameObject2);
				continue;
			}
			list2.AddRange(component.materials);
			for (int j = 0; j < component.sharedMesh.subMeshCount; j++)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = component.sharedMesh;
				item.subMeshIndex = j;
				list.Add(item);
				Transform[] bones = component.bones;
				foreach (Transform transform2 in bones)
				{
					Transform[] componentsInChildren = pObj.GetComponentsInChildren<Transform>();
					foreach (Transform transform3 in componentsInChildren)
					{
						if (transform3.name == transform2.name)
						{
							list3.Add(transform3);
							break;
						}
					}
				}
			}
			Object.Destroy(gameObject2);
		}
		SkinnedMeshRenderer component2 = pObj.GetComponent<SkinnedMeshRenderer>();
		if (component2.sharedMesh == null)
		{
			component2.sharedMesh = new Mesh();
			component2.sharedMesh.name = "PlayerAppearance0";
		}
		component2.sharedMesh.Clear();
		component2.sharedMesh.CombineMeshes(list.ToArray(), mergeSubMeshes: false, useMatrices: false);
		component2.bones = list3.ToArray();
		component2.materials = list2.ToArray();
		InitAppearance(pObj);
		ResetBadyData();
	}

	private void RebuildFace()
	{
		if (mBuildFaceMesh)
		{
		}
	}

	public void RebulidMesh(GameObject pTGameObject)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		List<Transform> list3 = new List<Transform>();
		AppearanceData.BodyData[] array = mCurrentBodyElement;
		foreach (AppearanceData.BodyData bodyData in array)
		{
			GameObject gameObject = Resources.Load(bodyData.mFilePath) as GameObject;
			if (!gameObject)
			{
				continue;
			}
			Transform transform = gameObject.transform.FindChild(bodyData.mName);
			if (!transform)
			{
				continue;
			}
			GameObject gameObject2 = Object.Instantiate(transform.gameObject);
			if (!gameObject2)
			{
				continue;
			}
			SkinnedMeshRenderer component = gameObject2.GetComponent<SkinnedMeshRenderer>();
			if (!component)
			{
				Object.Destroy(gameObject2);
				continue;
			}
			list2.AddRange(component.materials);
			for (int j = 0; j < component.sharedMesh.subMeshCount; j++)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = component.sharedMesh;
				item.subMeshIndex = j;
				list.Add(item);
				Transform[] bones = component.bones;
				foreach (Transform transform2 in bones)
				{
					Transform[] componentsInChildren = pTGameObject.GetComponentsInChildren<Transform>();
					foreach (Transform transform3 in componentsInChildren)
					{
						if (transform3.name == transform2.name)
						{
							list3.Add(transform3);
							break;
						}
					}
				}
			}
			Object.Destroy(gameObject2);
		}
		SkinnedMeshRenderer skinnedMeshRenderer = pTGameObject.GetComponent<SkinnedMeshRenderer>();
		if (null == skinnedMeshRenderer)
		{
			skinnedMeshRenderer = pTGameObject.AddComponent<SkinnedMeshRenderer>();
		}
		if (skinnedMeshRenderer.sharedMesh == null)
		{
			skinnedMeshRenderer.sharedMesh = new Mesh();
			skinnedMeshRenderer.sharedMesh.name = "PlayerAppearance1";
		}
		skinnedMeshRenderer.sharedMesh.Clear(keepVertexLayout: false);
		skinnedMeshRenderer.sharedMesh.CombineMeshes(list.ToArray(), mergeSubMeshes: false, useMatrices: false);
		skinnedMeshRenderer.bones = list3.ToArray();
		skinnedMeshRenderer.materials = list2.ToArray();
		skinnedMeshRenderer.updateWhenOffscreen = true;
		UpDateSkinColor(skinnedMeshRenderer);
	}

	private void UpDateSkinColor(SkinnedMeshRenderer render)
	{
		Material[] sharedMaterials = render.sharedMaterials;
		foreach (Material material in sharedMaterials)
		{
			if (material.name.Contains("eye"))
			{
				material.SetColor("_SkinColor", mAppearanceData.mEyeColor);
			}
			else if (material.name.Contains("hair"))
			{
				material.SetColor("_Color", mAppearanceData.mHairColor);
			}
			else if (material.name.Contains("head"))
			{
				material.SetColor("_Color", mAppearanceData.mSkinColor);
			}
			else if (!material.name.Contains("Helmet"))
			{
				material.SetColor("_SkinColor", mAppearanceData.mSkinColor);
			}
		}
	}

	public void SetBodyBone(int pPos, float sliderValue)
	{
		if (mSynchBodyBone == null)
		{
			return;
		}
		for (int i = 0; i < AppearanceData.mBodilyElements[pPos].Length - 2; i++)
		{
			Vector3 b = mAppearanceData.mBodyCustomDatas[pPos][i * 3];
			Vector3 a = mAppearanceData.mBodyCustomDatas[pPos][i * 3 + 1];
			Vector3 vector = mAppearanceData.mBodyCustomDatas[pPos][i * 3 + 2];
			Vector3 one = Vector3.one;
			one = ((!(sliderValue < 0.5f)) ? Vector3.Lerp(vector, b, (sliderValue - 0.5f) * 2f) : Vector3.Lerp(a, vector, sliderValue * 2f));
			if (mSynchBodyBone[pPos][i] == null)
			{
				break;
			}
			switch (AppearanceData.mBodilyElements[pPos][1])
			{
			case "Move":
				mSynchBodyBone[pPos][i].transform.localPosition = one;
				break;
			case "Scale":
				mSynchBodyBone[pPos][i].transform.localScale = one;
				break;
			case "Rotate":
				mSynchBodyBone[pPos][i].transform.localEulerAngles = one;
				break;
			}
			mAppearanceData.mCurrentBodyCustomData[pPos] = sliderValue;
		}
	}

	public void ChangeBodyBones(int pPos, float oldsliderValue, float newsliderValue)
	{
		for (int i = 0; i < AppearanceData.mBodilyElements[pPos].Length - 2; i++)
		{
			Vector3 b = mAppearanceData.mBodyCustomDatas[pPos][i * 3];
			Vector3 a = mAppearanceData.mBodyCustomDatas[pPos][i * 3 + 1];
			Vector3 vector = mAppearanceData.mBodyCustomDatas[pPos][i * 3 + 2];
			Vector3 one = Vector3.one;
			one = ((!(oldsliderValue < 0.5f)) ? Vector3.Lerp(vector, b, (oldsliderValue - 0.5f) * 2f) : Vector3.Lerp(a, vector, oldsliderValue * 2f));
			Vector3 one2 = Vector3.one;
			one2 = ((!(newsliderValue < 0.5f)) ? Vector3.Lerp(vector, b, (newsliderValue - 0.5f) * 2f) : Vector3.Lerp(a, vector, newsliderValue * 2f));
			Vector3 vector2 = one2 - one;
			if (mSynchBodyBone[pPos][i] == null)
			{
				break;
			}
			switch (AppearanceData.mBodilyElements[pPos][1])
			{
			case "Move":
				mSynchBodyBone[pPos][i].transform.localPosition += vector2;
				break;
			case "Scale":
				mSynchBodyBone[pPos][i].transform.localScale += vector2;
				break;
			case "Rotate":
				mSynchBodyBone[pPos][i].transform.localEulerAngles += vector2;
				break;
			}
			mAppearanceData.mCurrentBodyCustomData[pPos] = newsliderValue;
		}
	}
}
