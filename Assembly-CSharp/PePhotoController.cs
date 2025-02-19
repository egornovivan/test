using UnityEngine;

public class PePhotoController : ViewController
{
	public GameObject LightRoot;

	public override void SetTarget(Transform target)
	{
		base.SetTarget(target);
	}

	public Texture2D TakePhoto()
	{
		if (m_Target == null)
		{
			return null;
		}
		Transform transform = m_Target.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
		Transform parent = base.transform.parent;
		if (transform != null)
		{
			base.transform.parent = transform;
			base.transform.localPosition = m_Offset;
			base.transform.localRotation = m_Rot;
			LightRoot.transform.position = base.transform.position;
			LightRoot.transform.localPosition = new Vector3(LightRoot.transform.localPosition.x, LightRoot.transform.localPosition.y - 1f, LightRoot.transform.localPosition.z);
			Texture2D result = RTImage(_viewCam);
			base.transform.parent = parent;
			return result;
		}
		return null;
	}

	public static Texture2D RTImage(Camera cam)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = cam.targetTexture;
		cam.Render();
		Texture2D texture2D = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, mipmap: false);
		texture2D.ReadPixels(new Rect(0f, 0f, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		return texture2D;
	}
}
