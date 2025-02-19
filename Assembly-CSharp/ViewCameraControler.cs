using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ViewCameraControler : MonoBehaviour
{
	public enum ViewPart
	{
		VP_All,
		VP_Head,
		VP_BigHead
	}

	private ViewPart mViewPart;

	private RenderTexture mRenderTexture;

	private bool mGetPictrue;

	public bool mAlwaysActive;

	private GameObject mTargetObj;

	private BlurOptimized mBlurEffect;

	private Light[] mLight;

	public Vector3 moffset = new Vector3(-0.03f, 0.1f, -0.06f);

	private Transform mTargetTran;

	private Camera mCamera;

	public Camera viewCamera
	{
		get
		{
			if (mCamera == null)
			{
				viewCamera = GetComponent<Camera>();
			}
			return mCamera;
		}
		private set
		{
			mCamera = value;
		}
	}

	public static ViewCameraControler CreatViewCamera()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/Other/ViewCamera"), Vector3.one, Quaternion.identity) as GameObject;
		return gameObject.GetComponent<ViewCameraControler>();
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public void Init(bool alwaysActive)
	{
		GetComponent<Camera>().nearClipPlane = 0.01f;
		GetComponent<Camera>().farClipPlane = 3f;
		mGetPictrue = false;
		mBlurEffect = base.gameObject.AddComponent<BlurOptimized>();
		mBlurEffect.blurIterations = 0;
		mBlurEffect.blurSize = 0f;
		mLight = new Light[2];
		mLight[0] = base.transform.FindChild("Pointlight1").GetComponent<Light>();
		mLight[1] = base.transform.FindChild("Pointlight2").GetComponent<Light>();
		mAlwaysActive = alwaysActive;
		mRenderTexture = new RenderTexture(512, 512, 16);
		mRenderTexture.isCubemap = false;
		GetComponent<Camera>().targetTexture = mRenderTexture;
	}

	private void Update()
	{
		if (!mRenderTexture.IsCreated())
		{
			Photo();
			mRenderTexture.Create();
		}
		if (!mAlwaysActive && !mGetPictrue)
		{
			base.gameObject.SetActive(value: false);
		}
		if (mGetPictrue)
		{
			mGetPictrue = false;
		}
		if (null != mTargetTran && mViewPart != 0)
		{
			base.transform.LookAt(mTargetTran.position + 0.02f * Vector3.up, Vector3.up);
		}
	}

	public void SetTarget(GameObject targetObj, ViewPart viewPart = ViewPart.VP_Head)
	{
		mTargetObj = targetObj;
		SetViewPart(viewPart);
	}

	public RenderTexture GetTex()
	{
		return GetComponent<Camera>().targetTexture;
	}

	public void Photo()
	{
		mGetPictrue = true;
		base.gameObject.SetActive(value: true);
	}

	public void SetViewPart(ViewPart viewPart)
	{
		mViewPart = viewPart;
		switch (mViewPart)
		{
		case ViewPart.VP_All:
			mTargetTran = mTargetObj.transform;
			if (mTargetTran != null)
			{
				base.transform.parent = mTargetTran;
				base.transform.localPosition = new Vector3(0f, 0.9f, 2f);
			}
			GetComponent<Camera>().orthographic = false;
			GetComponent<Camera>().aspect = 95f / 116f;
			SetLightState(state: true);
			base.transform.LookAt(mTargetTran.position + Vector3.up * 0.9f, Vector3.up);
			mBlurEffect.enabled = false;
			break;
		case ViewPart.VP_Head:
			mTargetTran = mTargetObj.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
			if (mTargetTran != null)
			{
				base.transform.parent = mTargetTran;
				base.transform.localPosition = -1f * new Vector3(0.06378798f, -0.2264594f, 0.1151667f);
			}
			GetComponent<Camera>().orthographic = true;
			GetComponent<Camera>().orthographicSize = 0.19f;
			GetComponent<Camera>().aspect = 1f;
			base.transform.LookAt(mTargetTran.position, Vector3.up);
			mBlurEffect.enabled = false;
			break;
		case ViewPart.VP_BigHead:
			mTargetTran = mTargetObj.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
			if (mTargetTran != null)
			{
				base.transform.parent = mTargetTran;
				base.transform.localPosition = new Vector3(-0.06070369f, 0.6686818f, -0.03194972f);
				base.transform.localRotation = Quaternion.Euler(new Vector3(85.67319f, -0.7489014f, 86.21811f));
			}
			GetComponent<Camera>().orthographic = true;
			GetComponent<Camera>().orthographicSize = 0.12f;
			GetComponent<Camera>().aspect = 1f;
			SetLightState(state: true);
			mBlurEffect.enabled = true;
			base.transform.LookAt(mTargetTran.position, Vector3.up);
			break;
		}
	}

	public void SetLightState(bool state)
	{
		if (mLight != null)
		{
			mLight[0].enabled = state;
			mLight[1].enabled = state;
		}
	}
}
