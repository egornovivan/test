using UnityEngine;

public class PhotoStudio : MonoBehaviour
{
	private static PhotoStudio mInstance;

	public Camera mPhotoCam;

	public GameObject light;

	public static PhotoStudio Instance
	{
		get
		{
			if (mInstance == null && Application.isPlaying)
			{
				GameObject gameObject = Resources.Load<GameObject>("Prefab/Other/PhotoStudio");
				if (gameObject != null)
				{
					Object.Instantiate(gameObject);
				}
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		mInstance = this;
		light.SetActive(value: false);
		mPhotoCam.aspect = 1f;
		mPhotoCam.clearFlags = CameraClearFlags.Color;
		mPhotoCam.targetTexture = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);
		mPhotoCam.gameObject.SetActive(value: false);
	}

	public Texture2D TakePhoto(GameObject obj, int sex)
	{
		string text = "Prefab/Npc/";
		switch (sex)
		{
		case 1:
			text += "RandomNpcViewFemale";
			break;
		case 2:
			text += "RandomNpcView";
			break;
		}
		GameObject gameObject = Object.Instantiate(Resources.Load(text)) as GameObject;
		if (null == gameObject)
		{
			return null;
		}
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		GameObject gameObject2 = Object.Instantiate(obj);
		Component[] components = gameObject2.GetComponents<Component>();
		Component[] array = components;
		foreach (Component component in array)
		{
			if (component.GetType() != typeof(SkinnedMeshRenderer) && component.GetType() != typeof(Animator) && component.GetType() != typeof(Transform))
			{
				Object.Destroy(component);
			}
		}
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one;
		gameObject2.layer = 9;
		SkinnedMeshRenderer[] componentsInChildren = gameObject2.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		SkinnedMeshRenderer[] array2 = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array2)
		{
			skinnedMeshRenderer.gameObject.layer = 9;
			if (!skinnedMeshRenderer.enabled)
			{
				skinnedMeshRenderer.enabled = true;
			}
		}
		gameObject.SetActive(value: true);
		Transform transform = gameObject2.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
		if (transform != null)
		{
			mPhotoCam.transform.parent = transform;
			mPhotoCam.transform.localPosition = new Vector3(-0.01f, 0.213f, -0.155f);
			mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(54.1671f, 344.2581f, 77.74331f));
		}
		light.transform.position = mPhotoCam.transform.position;
		light.transform.localPosition = new Vector3(light.transform.localPosition.x, light.transform.localPosition.y - 1f, light.transform.localPosition.z);
		light.SetActive(value: true);
		Texture2D result = RTImage(mPhotoCam);
		light.SetActive(value: false);
		mPhotoCam.transform.parent = base.transform;
		Object.DestroyImmediate(gameObject);
		return result;
	}

	public Texture2D TakePhoto1(GameObject obj, int sex)
	{
		string text = "Prefab/Npc/";
		switch (sex)
		{
		case 1:
			text += "RandomNpcViewFemale";
			break;
		case 2:
			text += "RandomNpcView";
			break;
		}
		GameObject gameObject = Object.Instantiate(Resources.Load(text)) as GameObject;
		if (null == gameObject)
		{
			return null;
		}
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		GameObject gameObject2 = Object.Instantiate(obj);
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		gameObject2.transform.localScale = Vector3.one;
		gameObject2.layer = 9;
		gameObject.SetActive(value: true);
		SkinnedMeshRenderer[] componentsInChildren = gameObject2.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (!skinnedMeshRenderer.enabled)
			{
				skinnedMeshRenderer.enabled = true;
			}
		}
		Transform transform = gameObject2.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
		if (transform != null)
		{
			mPhotoCam.transform.parent = transform;
			mPhotoCam.transform.localPosition = new Vector3(-0.06070369f, 0.6686818f, -0.03194972f);
			mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(87.07319f, -0.7489014f, 86.21811f));
		}
		light.transform.position = mPhotoCam.transform.position;
		light.SetActive(value: true);
		return RTImage(mPhotoCam);
	}

	public static Texture2D RTImage(Camera cam)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = cam.targetTexture;
		cam.Render();
		Texture2D texture2D = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, mipmap: false);
		texture2D.ReadPixels(new Rect(0f, 0f, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		return texture2D;
	}
}
