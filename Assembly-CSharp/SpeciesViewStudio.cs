using System;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using UnityEngine;
using WhiteCat;

public class SpeciesViewStudio : MonoBehaviour
{
	private static SpeciesViewStudio m_Instance;

	private int m_IDInitial;

	public static SpeciesViewStudio Instance
	{
		get
		{
			if (m_Instance == null && Application.isPlaying)
			{
				GameObject gameObject = Resources.Load<GameObject>("Prefab/SpeciesViewStudio");
				if (gameObject != null)
				{
					m_Instance = UnityEngine.Object.Instantiate(gameObject).GetComponent<SpeciesViewStudio>();
				}
			}
			return m_Instance;
		}
	}

	public static PeViewController CreateViewController(ViewControllerParam param)
	{
		GameObject gameObject = Resources.Load("Prefab/ViewController") as GameObject;
		if (gameObject == null)
		{
			throw new Exception("Load view camera cursor prefab failed");
		}
		PeViewController component = gameObject.GetComponent<PeViewController>();
		if (component == null)
		{
			throw new Exception("Load iso cursor prefab failed");
		}
		PeViewController peViewController = UnityEngine.Object.Instantiate(component);
		peViewController.transform.SetParent(Instance.transform);
		peViewController.transform.localPosition = Vector3.zero;
		peViewController.transform.localScale = Vector3.one;
		peViewController.transform.localRotation = Quaternion.identity;
		peViewController.SetParam(param);
		peViewController.ID = Instance.m_IDInitial;
		Instance.m_IDInitial++;
		return peViewController;
	}

	public static GameObject LoadMonsterModelByID(int modelID, ref MovementField movementField)
	{
		MonsterProtoDb.Item item = MonsterProtoDb.Get(modelID);
		if (item == null)
		{
			return null;
		}
		string modelPath = item.modelPath;
		if (string.IsNullOrEmpty(modelPath))
		{
			return null;
		}
		GameObject modelGo = AssetsLoader.Instance.InstantiateAssetImm(modelPath, Vector3.zero, Quaternion.identity, Vector3.one);
		modelGo = GetOnlyModel(modelGo);
		if (modelGo == null)
		{
			return null;
		}
		ResetModel(modelGo, item.movementField);
		movementField = item.movementField;
		return modelGo;
	}

	public static GameObject GetOnlyModel(GameObject modelGo)
	{
		if (null == modelGo)
		{
			return null;
		}
		Animator componentInChildren = modelGo.GetComponentInChildren<Animator>();
		if (null == componentInChildren)
		{
			return null;
		}
		componentInChildren.transform.SetParent(modelGo.transform.parent);
		UnityEngine.Object.Destroy(modelGo);
		return componentInChildren.gameObject;
	}

	public static void ResetModel(GameObject modelGo, MovementField movementField)
	{
		modelGo.name = "CloneModel";
		modelGo.transform.parent = m_Instance.transform;
		modelGo.transform.localPosition = Vector3.zero;
		modelGo.transform.localRotation = Quaternion.identity;
		modelGo.transform.localRotation = Quaternion.identity;
		modelGo.layer = 9;
		Animator componentInChildren = modelGo.GetComponentInChildren<Animator>();
		if (null != componentInChildren)
		{
			string text = ((movementField != MovementField.Sky) ? "normal_leisure" : "normalsky_leisure");
			if (movementField == MovementField.Sky)
			{
				componentInChildren.SetBool("Fly", value: true);
			}
			componentInChildren.SetTrigger(text + "0");
			AnimatorClipInfo[] currentAnimatorClipInfo = componentInChildren.GetCurrentAnimatorClipInfo(0);
			if (currentAnimatorClipInfo != null && currentAnimatorClipInfo.Length > 0)
			{
				currentAnimatorClipInfo[0].clip.SampleAnimation(componentInChildren.gameObject, currentAnimatorClipInfo[0].clip.length * 0.5f);
			}
		}
		ClearModel(modelGo);
	}

	public static void ClearModel(GameObject viewGo)
	{
		List<Transform> list = viewGo.GetComponentsInChildren<Transform>().ToList();
		list.Add(viewGo.transform);
		for (int i = 0; i < list.Count; i++)
		{
			Transform transform = list[i];
			Component[] components = transform.gameObject.GetComponents<Component>();
			Component[] array = components;
			foreach (Component component in array)
			{
				if (component.GetType() != typeof(SkinnedMeshRenderer) && component.GetType() != typeof(SkinnedMeshRendererHelper) && component.GetType() != typeof(Animator) && component.GetType() != typeof(Transform) && component.GetType() != typeof(MeshRenderer))
				{
					UnityEngine.Object.Destroy(component);
				}
			}
			transform.gameObject.layer = 9;
		}
	}

	public static void GetCanViewModelFullDistance(GameObject modelGo, Camera viewCamera, Vector2 viewWndSize, out float distance, out float yaw)
	{
		Vector3 size = GetModelBounds(modelGo).size;
		float num = Mathf.Sqrt(size.x * size.x + size.z * size.z);
		float num2;
		float num3;
		if (size.y > num)
		{
			num2 = size.y;
			num3 = viewWndSize.y * 0.75f;
		}
		else
		{
			num2 = num;
			num3 = viewWndSize.x * 0.75f;
		}
		yaw = (0f - Mathf.Acos(size.x / num)) * 57.29578f;
		distance = num2 * (float)viewCamera.pixelHeight * 0.5f / (num3 * Mathf.Tan(viewCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)));
	}

	public static Bounds GetModelBounds(GameObject modelGo)
	{
		SkinnedMeshRenderer componentInChildren = modelGo.GetComponentInChildren<SkinnedMeshRenderer>();
		Bounds result = default(Bounds);
		if (componentInChildren == null)
		{
			return result;
		}
		componentInChildren.updateWhenOffscreen = true;
		return componentInChildren.bounds;
	}
}
