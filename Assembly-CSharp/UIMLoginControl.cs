using CustomData;
using UnityEngine;

public class UIMLoginControl : MonoBehaviour
{
	public delegate void OnClickFun();

	public N_ImageButton btnCreate;

	public N_ImageButton btnDelete;

	public UILabel lbRoleName;

	public RolesControl rc;

	public Transform[] roleTransform;

	public OnClickFun BtnCreate;

	public OnClickFun BtnDelete;

	public OnClickFun BtnEdit;

	public OnClickFun BtnEnter;

	public OnClickFun BtnBack;

	public OnClickFun BtnOK;

	public OnClickFun BtnCancel;

	public float testNamePos_Y = 4f;

	private int deleteRoleIndex;

	private void Start()
	{
	}

	private void Update()
	{
		UpdateButtonState();
		UpdateRolesName();
	}

	public void UpdateRolesName()
	{
		if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) != null)
		{
			if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).name != lbRoleName.text)
			{
				lbRoleName.text = MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).name;
			}
		}
		else
		{
			lbRoleName.text = string.Empty;
		}
	}

	private void UpdateButtonState()
	{
		if (MLPlayerInfo.Instance.GetRoleListNotNullCount() == 0 && btnDelete.isEnabled)
		{
			EnableButton(btnDelete, value: false);
		}
		else if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) == null && btnDelete.isEnabled)
		{
			EnableButton(btnDelete, value: false);
		}
		else if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) != null && !btnDelete.isEnabled)
		{
			EnableButton(btnDelete, value: true);
		}
		if (MLPlayerInfo.Instance.GetRoleListNotNullCount() == MLPlayerInfo.Instance.GetMaxRoleCount())
		{
			if (btnCreate.isEnabled)
			{
				EnableButton(btnCreate, value: false);
			}
		}
		else if (!btnCreate.isEnabled)
		{
			EnableButton(btnCreate, value: true);
		}
	}

	private void BtnCreateOnClick()
	{
		Debug.Log("btnCreate OnClick");
		if (MLPlayerInfo.Instance.GetRoleListNotNullCount() != MLPlayerInfo.Instance.GetMaxRoleCount())
		{
			GameClientLobby.Self.TryCreateRole();
			if (BtnCreate != null)
			{
				BtnCreate();
			}
		}
	}

	private void BtnDeleteOnClick()
	{
		Debug.Log("btnDelete OnClick");
		if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) != null)
		{
			deleteRoleIndex = rc.GetSelectedIndex();
			string text = PELocalization.GetString(8000590) + MLPlayerInfo.Instance.GetRoleInfo(deleteRoleIndex).name + "'?";
			MessageBox_N.ShowYNBox(text, BtnOKOnClick, BtnCancelOnClick);
			if (BtnDelete != null)
			{
				BtnDelete();
			}
		}
	}

	private void BtnEditOnClick()
	{
		if (BtnEdit != null)
		{
			BtnEdit();
		}
	}

	private void BtnEnterOnClick()
	{
		if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) != null)
		{
			rc.TakeRoleHerderTexture();
			if (BtnEnter != null)
			{
				BtnEnter();
			}
			Invoke("EnterLobby", 0.1f);
		}
	}

	private void EnterLobby()
	{
		if (false)
		{
			RoleInfo mRoleInfo = MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).mRoleInfo;
			if (!SystemSettingData.Instance.Tutorialed && mRoleInfo.name.Equals("tutorial"))
			{
				string text = PELocalization.GetString(8000986) + "'?";
				MessageBox_N.ShowYNBox(text, BtnYOnClick, BtnNoOnClick);
			}
			else
			{
				BtnNoOnClick();
			}
		}
		else if (!SystemSettingData.Instance.Tutorialed)
		{
			string text2 = PELocalization.GetString(8000986) + "'?";
			MessageBox_N.ShowYNBox(text2, BtnYOnClick, BtnNoOnClick);
		}
		else
		{
			BtnNoOnClick();
		}
	}

	private void BtnYOnClick()
	{
		PeSceneCtrl.Instance.GotToTutorial(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).mRoleInfo);
	}

	private void BtnNoOnClick()
	{
		GameClientLobby.Self.TryEnterLobby(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).mRoleInfo.roleID);
	}

	private void BtnBackOnClick()
	{
		PeSceneCtrl.Instance.GotoMainMenuScene();
		if (BtnBack != null)
		{
			BtnBack();
		}
	}

	private void BtnOKOnClick()
	{
		GameClientLobby.Self.TryDeleteRole(MLPlayerInfo.Instance.GetRoleInfo(deleteRoleIndex).mRoleInfo.roleID);
		if (BtnOK != null)
		{
			BtnOK();
		}
	}

	private void BtnCancelOnClick()
	{
		if (BtnCancel != null)
		{
			BtnCancel();
		}
	}

	private void EnableButton(N_ImageButton btn, bool value)
	{
		if (value)
		{
			btn.isEnabled = true;
		}
		else
		{
			btn.isEnabled = false;
		}
	}
}
