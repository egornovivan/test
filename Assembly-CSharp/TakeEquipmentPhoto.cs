using Pathea;
using UnityEngine;

public class TakeEquipmentPhoto
{
	public GameObject mEqPhoto;

	public GameObject mViewObj;

	public ViewCameraControler mBodyViewCtr;

	private Vector3 modelAngle = Vector3.zero;

	public Texture PhotoTex => mBodyViewCtr.GetTex();

	public void Init(Vector3 target)
	{
		if (mEqPhoto == null)
		{
			mEqPhoto = new GameObject("EqPhotoObj");
		}
		mEqPhoto.transform.parent = UIOthers.Inctence.transform;
		mEqPhoto.transform.localScale = Vector3.one;
		mEqPhoto.transform.localPosition = target;
		mBodyViewCtr = ViewCameraControler.CreatViewCamera();
		mBodyViewCtr.Init(alwaysActive: true);
		mBodyViewCtr.SetLightState(state: true);
		mBodyViewCtr.SetActive(active: false);
	}

	public void ResetViewCmpt(BiologyViewCmpt viewCmpt)
	{
		if (viewCmpt == null)
		{
			return;
		}
		if (mViewObj != null)
		{
			modelAngle = mViewObj.transform.localRotation.eulerAngles;
			Object.Destroy(mViewObj);
		}
		mViewObj = viewCmpt.CloneModel();
		if (mViewObj == null)
		{
			return;
		}
		mViewObj.name = "Player";
		mViewObj.transform.parent = mEqPhoto.transform;
		mViewObj.transform.localPosition = Vector3.zero;
		mViewObj.transform.localRotation = Quaternion.Euler(modelAngle);
		mViewObj.layer = 9;
		Renderer[] componentsInChildren = mViewObj.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = ((componentsInChildren[i].gameObject.layer != 19) ? 9 : 20);
		}
		Projector[] componentsInChildren2 = mViewObj.GetComponentsInChildren<Projector>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].gameObject.layer = 20;
		}
		mBodyViewCtr.SetTarget(mViewObj, ViewCameraControler.ViewPart.VP_All);
		Camera component = mBodyViewCtr.GetComponent<Camera>();
		component.depth = 0f;
		component.transform.parent = null;
		component.transform.position = mViewObj.transform.position + new Vector3(0f, 1.31f, 1.883728f);
		component.transform.localEulerAngles = new Vector3(11f, 180f, 0f);
		component.cullingMask = 1049088;
		component.nearClipPlane = 0.1f;
		try
		{
			Component[] componentsInChildren3 = mViewObj.GetComponentsInChildren<Component>(includeInactive: true);
			Component[] array = componentsInChildren3;
			foreach (Component component2 in array)
			{
				if (null == component2 as Animator && null == component2 as SkinnedMeshRenderer && null == component2 as Transform)
				{
					Object.Destroy(component2);
				}
			}
		}
		catch
		{
			Debug.Log("delect equipment modle cmpt error!");
		}
	}
}
