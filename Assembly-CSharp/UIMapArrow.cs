using PeMap;
using UnityEngine;

public class UIMapArrow : MonoBehaviour
{
	public enum EArrowType
	{
		Main,
		Other
	}

	public ILabel trackLabel;

	public float visualWidth = 190f;

	public float visualHeight = 200f;

	public GameObject content;

	public void SetLabel(ILabel label, EArrowType arrow_type)
	{
		trackLabel = label;
	}

	private void UpdateTrans()
	{
		Vector3 vector = trackLabel.GetPos() - GameUI.Instance.mMainPlayer.position;
		Vector3 vector2 = vector;
		vector2.y = 0f;
		float num = vector.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x;
		float num2 = vector.z * GameUI.Instance.mUIMinMapCtrl.mMapScale.y;
		float num3 = 5f;
		if (num >= (0f - visualWidth) * 0.5f - num3 && num <= visualWidth * 0.5f + num3 && num2 >= (0f - visualHeight) * 0.5f - num3 && num2 <= visualHeight * 0.5f + num3)
		{
			if (content.activeSelf)
			{
				content.SetActive(value: false);
			}
		}
		else if (!content.activeSelf)
		{
			content.SetActive(value: true);
		}
		float num4 = 0f;
		float num5 = 0f;
		if (Mathf.Abs(vector2.x) > Mathf.Abs(vector2.z))
		{
			Vector3 vector3 = vector2;
			vector3.x = Mathf.Sign(vector3.x);
			vector3.z = vector2.z / (vector3.x * vector2.x);
			num4 = ((!(vector3.x < 0f)) ? ((visualWidth * 0.5f - 45f) * vector3.x) : ((visualWidth * 0.5f - 10f) * vector3.x));
			num5 = ((!(vector3.z < 0f)) ? ((visualHeight * 0.5f - 20f) * vector3.z) : ((visualHeight * 0.5f - 20f) * vector3.z));
		}
		else if (Mathf.Abs(vector2.x) < Mathf.Abs(vector2.z))
		{
			Vector3 vector4 = vector2;
			vector4.z = Mathf.Sign(vector4.z);
			vector4.x = vector2.x / (vector4.z * vector2.z);
			num4 = ((!(vector4.x < 0f)) ? ((visualWidth * 0.5f - 45f) * vector4.x) : ((visualWidth * 0.5f - 10f) * vector4.x));
			num5 = ((!(vector4.z < 0f)) ? ((visualHeight * 0.5f - 20f) * vector4.z) : ((visualHeight * 0.5f - 20f) * vector4.z));
		}
		else
		{
			num4 = 0f;
			num5 = 0f;
		}
		base.transform.localPosition = new Vector3(num4, num5, 0f);
		Vector3 eulerAngles = Quaternion.LookRotation(vector2).eulerAngles;
		base.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, Mathf.FloorToInt(0f - eulerAngles.y)));
	}

	private void Update()
	{
		if (trackLabel != null && GameUI.Instance.mMainPlayer != null)
		{
			UpdateTrans();
		}
	}
}
