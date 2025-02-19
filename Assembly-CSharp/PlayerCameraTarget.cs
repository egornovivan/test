using Pathea;
using UnityEngine;

public class PlayerCameraTarget : MonoBehaviour
{
	private Transform _pelvis;

	private Transform _custom_camtar;

	public Transform CustomCameraTarget
	{
		get
		{
			_pelvis = Utils.GetChild(base.transform, "Bip01 Pelvis");
			string text = "CamTar";
			Transform transform = base.transform.FindChild(text);
			if (null == transform)
			{
				GameObject gameObject = new GameObject(text);
				transform = gameObject.transform;
				transform.parent = base.transform;
				transform.localPosition = new Vector3(0f, 0f, 0f);
				transform.localRotation = Quaternion.identity;
				_custom_camtar = transform;
			}
			return transform;
		}
	}

	public Transform ShootCameraTarget
	{
		get
		{
			string text = "ShootCamTar";
			Transform transform = base.transform.FindChild(text);
			if (null == transform)
			{
				GameObject gameObject = new GameObject(text);
				transform = gameObject.transform;
				transform.parent = base.transform;
				transform.localPosition = new Vector3(0.557f, 1.727f, 0.592f);
				transform.localRotation = Quaternion.identity;
			}
			return transform;
		}
	}

	public Transform ClimbCameraTarget => Utils.GetChild(base.transform, "Bip01 Spine3");

	private void Update()
	{
		if (_pelvis != null && _custom_camtar != null)
		{
			Vector3 vector = _pelvis.position - base.transform.position;
			vector.x *= 0.5f;
			vector.y *= 0.5f;
			float num = 1.23f;
			float num2 = 0f;
			if (PECameraMan.Instance.ControlType == 3)
			{
				num = 1.58f;
				num2 = 0.3f;
			}
			_custom_camtar.position = base.transform.position + vector * 0.1f + Vector3.up * num + base.transform.forward * num2;
			_custom_camtar.rotation = base.transform.rotation;
		}
	}
}
