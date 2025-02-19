using System;
using Pathea;
using UnityEngine;
using WhiteCat;

public class PeViewStudio : MonoBehaviour
{
	private static PeViewStudio _self;

	private int m_IDInitial;

	[SerializeField]
	private PePhotoController _photoController;

	public static Vector3 s_HeadPhotoPos = new Vector3(-0.01f, 0.213f, -0.155f);

	public static Quaternion s_HeadPhotoRot = Quaternion.Euler(54.1671f, 344.2581f, 77.74331f);

	public static Vector3 s_ViewPos = new Vector3(0f, 1.27f, 1.8f);

	public static PeViewStudio Self
	{
		get
		{
			if (_self == null && Application.isPlaying)
			{
				GameObject gameObject = Resources.Load<GameObject>("Prefab/ViewStudio");
				if (gameObject != null)
				{
					UnityEngine.Object.Instantiate(gameObject);
				}
			}
			return _self;
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
		peViewController.transform.localPosition = Vector3.zero;
		peViewController.transform.localScale = Vector3.one;
		peViewController.transform.localRotation = Quaternion.identity;
		peViewController.SetParam(param);
		peViewController.ID = Self.m_IDInitial;
		Self.m_IDInitial++;
		return peViewController;
	}

	public static GameObject CloneCharacterViewModel(BiologyViewCmpt viewCmpt, bool takePhoto = false)
	{
		if (viewCmpt == null)
		{
			return null;
		}
		GameObject gameObject = viewCmpt.CloneModel();
		if (gameObject == null)
		{
			return null;
		}
		ResetCloneModel(gameObject, takePhoto);
		return gameObject;
	}

	public static GameObject CloneModelObject(GameObject obj, bool takePhoto = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(obj);
		if (gameObject == null)
		{
			return null;
		}
		ResetCloneModel(gameObject, takePhoto);
		return gameObject;
	}

	private static void ResetCloneModel(GameObject viewGameObj, bool takePhoto)
	{
		viewGameObj.name = "Clone Mesh";
		viewGameObj.transform.parent = Self.transform;
		viewGameObj.transform.localPosition = Vector3.zero;
		viewGameObj.layer = 9;
		Animator componentInChildren = viewGameObj.GetComponentInChildren<Animator>();
		if ((bool)componentInChildren)
		{
			AnimatorClipInfo[] currentAnimatorClipInfo = componentInChildren.GetCurrentAnimatorClipInfo(0);
			for (int i = 0; i < currentAnimatorClipInfo.Length; i++)
			{
				currentAnimatorClipInfo[i].clip.SampleAnimation(viewGameObj, currentAnimatorClipInfo[i].clip.length * 0.5f);
			}
		}
		CleanUpModel(viewGameObj);
		if (takePhoto)
		{
			SkinnedMeshRenderer componentInChildren2 = viewGameObj.GetComponentInChildren<SkinnedMeshRenderer>();
			if ((bool)componentInChildren2)
			{
				Mesh mesh = new Mesh();
				componentInChildren2.BakeMesh(mesh);
				componentInChildren2.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
				MeshRenderer meshRenderer = componentInChildren2.gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterials = componentInChildren2.sharedMaterials;
				meshRenderer.useLightProbes = false;
				componentInChildren2.enabled = false;
				UnityEngine.Object.DestroyImmediate(componentInChildren2);
			}
		}
	}

	private static void CleanUpModel(GameObject viewObj)
	{
		MonoBehaviour[] componentsInChildren = viewObj.GetComponentsInChildren<MonoBehaviour>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] is ICloneModelHelper)
			{
				(componentsInChildren[i] as ICloneModelHelper).ResetView();
			}
		}
		Renderer[] componentsInChildren2 = viewObj.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren2;
		foreach (Renderer renderer in array)
		{
			Component[] components = renderer.gameObject.GetComponents<Component>();
			Component[] array2 = components;
			foreach (Component component in array2)
			{
				if (component.GetType() != typeof(SkinnedMeshRenderer) && component.GetType() != typeof(SkinnedMeshRendererHelper) && component.GetType() != typeof(Animator) && component.GetType() != typeof(Transform) && component.GetType() != typeof(MeshRenderer) && component.GetType() != typeof(ArmorBones))
				{
					UnityEngine.Object.Destroy(component);
				}
			}
			renderer.gameObject.layer = ((renderer.gameObject.layer != 19) ? 9 : 20);
		}
		Projector[] componentsInChildren3 = viewObj.GetComponentsInChildren<Projector>();
		for (int l = 0; l < componentsInChildren3.Length; l++)
		{
			componentsInChildren3[l].gameObject.layer = 20;
		}
	}

	public static Texture2D TakePhoto(BiologyViewCmpt viewCmpt, int width, int height, Vector3 local_pos, Quaternion local_rot)
	{
		GameObject gameObject = CloneCharacterViewModel(viewCmpt, takePhoto: true);
		if (gameObject == null)
		{
			return null;
		}
		return DoTakePhoto(gameObject, width, height, local_pos, local_rot);
	}

	public static Texture2D TakePhoto(GameObject model, int width, int height, Vector3 local_pos, Quaternion local_rot)
	{
		GameObject gameObject = CloneModelObject(model, takePhoto: true);
		if (gameObject == null)
		{
			return null;
		}
		return DoTakePhoto(gameObject, width, height, local_pos, local_rot);
	}

	private static Texture2D DoTakePhoto(GameObject cloneGo, int width, int height, Vector3 local_pos, Quaternion local_rot)
	{
		ViewControllerParam defaultPortrait = ViewControllerParam.DefaultPortrait;
		defaultPortrait.texWidth = width;
		defaultPortrait.texHeight = height;
		cloneGo.transform.localPosition = new Vector3(-500f, 0f, 0f);
		Self._photoController.Set(cloneGo.transform, defaultPortrait);
		Self._photoController.SetLocalTrans(local_pos, local_rot);
		Self._photoController.gameObject.SetActive(value: true);
		Texture2D result = Self._photoController.TakePhoto();
		Self._photoController.gameObject.SetActive(value: false);
		cloneGo.SetActive(value: false);
		return result;
	}

	private void Awake()
	{
		if (_self != null)
		{
			throw new Exception("Only one viewStudio is exist");
		}
		_self = this;
		_photoController.gameObject.SetActive(value: false);
	}
}
