using UnityEngine;

public class VCEMsgBox : MonoBehaviour
{
	public VCEMsgBoxType m_Type;

	public int m_PopupFrame;

	public GameObject m_ButtonL;

	public GameObject m_ButtonC;

	public GameObject m_ButtonR;

	public UILabel m_Title;

	public UILabel m_Message;

	public UISprite m_Icon;

	public UITweener m_MsgBoxTween;

	private VCEMsgBoxButton m_RespButton;

	public static string s_MsgBoxResPath = "GUI/Prefabs/VCE Message Box";

	public static string s_MsgBoxGroupName = "VCE Message Box Group";

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnLBtnClick()
	{
		m_ButtonL.SetActive(value: false);
		m_ButtonR.SetActive(value: false);
		m_ButtonC.SetActive(value: false);
		m_RespButton = VCEMsgBoxButton.L;
		m_MsgBoxTween.Play(forward: false);
	}

	public void OnCBtnClick()
	{
		m_ButtonL.SetActive(value: false);
		m_ButtonR.SetActive(value: false);
		m_ButtonC.SetActive(value: false);
		m_RespButton = VCEMsgBoxButton.C;
		m_MsgBoxTween.Play(forward: false);
	}

	public void OnRBtnClick()
	{
		m_ButtonL.SetActive(value: false);
		m_ButtonR.SetActive(value: false);
		m_ButtonC.SetActive(value: false);
		m_RespButton = VCEMsgBoxButton.R;
		m_MsgBoxTween.Play(forward: false);
	}

	public void OnCollapse()
	{
		if (m_MsgBoxTween.transform.localScale.magnitude < 0.1f)
		{
			VCEMsgBoxResponse.Response(m_Type, m_RespButton, m_PopupFrame);
			Object.Destroy(base.gameObject);
		}
	}

	public static void Show(VCEMsgBoxType type)
	{
		GameObject gameObject = GameObject.Find(s_MsgBoxGroupName);
		if (gameObject != null)
		{
			GameObject gameObject2 = Object.Instantiate(Resources.Load(s_MsgBoxResPath) as GameObject);
			Vector3 localScale = gameObject2.transform.localScale;
			gameObject2.name = "Message Box " + Time.frameCount + " (" + type.ToString() + ")";
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = localScale;
			VCEMsgBoxDesc vCEMsgBoxDesc = new VCEMsgBoxDesc(type);
			VCEMsgBox component = gameObject2.GetComponent<VCEMsgBox>();
			component.m_Type = type;
			component.m_PopupFrame = Time.frameCount;
			component.m_Title.text = vCEMsgBoxDesc.Title;
			component.m_Message.text = vCEMsgBoxDesc.Message;
			component.m_Icon.spriteName = vCEMsgBoxDesc.Icon;
			component.m_ButtonL.GetComponentsInChildren<UILabel>(includeInactive: true)[0].text = vCEMsgBoxDesc.ButtonL;
			component.m_ButtonR.GetComponentsInChildren<UILabel>(includeInactive: true)[0].text = vCEMsgBoxDesc.ButtonR;
			component.m_ButtonC.GetComponentsInChildren<UILabel>(includeInactive: true)[0].text = vCEMsgBoxDesc.ButtonC;
			if (vCEMsgBoxDesc.ButtonL.Length > 0)
			{
				component.m_ButtonL.SetActive(value: true);
			}
			else
			{
				component.m_ButtonL.SetActive(value: false);
			}
			if (vCEMsgBoxDesc.ButtonR.Length > 0)
			{
				component.m_ButtonR.SetActive(value: true);
			}
			else
			{
				component.m_ButtonR.SetActive(value: false);
			}
			if (vCEMsgBoxDesc.ButtonC.Length > 0)
			{
				component.m_ButtonC.SetActive(value: true);
			}
			else
			{
				component.m_ButtonC.SetActive(value: false);
			}
			if (type == VCEMsgBoxType.DELETE_ISO)
			{
				component.m_ButtonL.GetComponentsInChildren<UILabel>(includeInactive: true)[0].color = Color.red;
			}
			gameObject2.SetActive(value: true);
		}
	}
}
