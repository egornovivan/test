using UnityEngine;

public class UIAdminstratorItem : MonoBehaviour
{
	public delegate void ItemAdminOnClick(object sender, UserAdmin mUserAdmin);

	public delegate void ItemAdminOnpitch(object sender, UserAdmin mUserAdmin, bool Ispitch);

	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UILabel mLbSet;

	[SerializeField]
	public UILabel mLbForbidden;

	[SerializeField]
	private GameObject mAdminstratorBg;

	[SerializeField]
	private GameObject mAdminstratorPitchBg;

	[SerializeField]
	private GameObject mISPrivilegesBg;

	[SerializeField]
	private GameObject mNotPrivilegesBg;

	[SerializeField]
	public GameObject mSetBtn;

	[SerializeField]
	public GameObject mForbidenBtn;

	private UserAdmin _mUserAdmin;

	public bool isForbiddenRelsh;

	private static bool isForbidden;

	private bool Ispitch;

	public string NameText
	{
		get
		{
			return mLbName.text;
		}
		set
		{
			mLbName.text = value;
		}
	}

	public UserAdmin mUserAdmin
	{
		get
		{
			return _mUserAdmin;
		}
		set
		{
			_mUserAdmin = value;
		}
	}

	public event ItemAdminOnClick e_ItemAdminOnClick;

	public event ItemAdminOnpitch e_ItemAdminOnpitch;

	private void Start()
	{
		IntShow();
		ServerAdministrator.BuildLockChangedEvent = OnBuildLockChanged;
		ServerAdministrator.PlayerBanChangedEvent = OnPlayerBanChanged;
	}

	private void OnDestroy()
	{
		ServerAdministrator.BuildLockChangedEvent = null;
		ServerAdministrator.PlayerBanChangedEvent = null;
	}

	private void OnBuildLockChanged(bool Build)
	{
		BuildShow(Build);
		UIAdminstratorctr.ChangeAssistant(_mUserAdmin);
	}

	private void OnPlayerBanChanged(bool ban)
	{
	}

	private void IntShow()
	{
		if (_mUserAdmin.HasPrivileges(AdminMask.AssistRole))
		{
			mISPrivilegesBg.SetActive(value: true);
			mNotPrivilegesBg.SetActive(value: false);
		}
		else
		{
			mISPrivilegesBg.SetActive(value: false);
			mNotPrivilegesBg.SetActive(value: true);
		}
	}

	private void Update()
	{
	}

	public void PrivilegesShow(bool IS, bool Not)
	{
		mISPrivilegesBg.SetActive(IS);
		mNotPrivilegesBg.SetActive(Not);
		if (IS)
		{
			mLbSet.text = "ReSet";
		}
		else
		{
			mLbSet.text = "Set";
		}
	}

	public void BuildShow(bool Lock)
	{
		if (Lock)
		{
			mLbForbidden.text = "BuildLock";
		}
		else
		{
			mLbForbidden.text = "Build";
		}
	}

	private void OnSetBtn()
	{
		if (mLbSet.text == "Set")
		{
			ServerAdministrator.RequestAddAssistants(_mUserAdmin.Id);
		}
		else
		{
			ServerAdministrator.RequestDeleteAssistants(_mUserAdmin.Id);
		}
	}

	private void RefalshPrivileges()
	{
		if (ServerAdministrator.IsAssistant(_mUserAdmin.Id))
		{
			mISPrivilegesBg.SetActive(value: true);
			mNotPrivilegesBg.SetActive(value: false);
			mLbSet.text = "ReSet";
		}
		else
		{
			mISPrivilegesBg.SetActive(value: false);
			mNotPrivilegesBg.SetActive(value: true);
			mLbSet.text = "Set";
		}
	}

	public void MoveToBlackList(int MovedId)
	{
		if (MovedId == _mUserAdmin.Id)
		{
			mSetBtn.SetActive(value: false);
			mForbidenBtn.SetActive(value: false);
		}
	}

	private void ReFlshForBidden()
	{
		if (isForbiddenRelsh)
		{
			if (mLbForbidden.text == "Build")
			{
				isForbidden = true;
				ServerAdministrator.RequestBuildLock(_mUserAdmin.Id);
			}
			else
			{
				isForbidden = false;
				ServerAdministrator.RequestBuildUnLock(_mUserAdmin.Id);
			}
			isForbiddenRelsh = false;
		}
	}

	private void OnForbiddenBtn()
	{
		isForbiddenRelsh = true;
		isForbidden = !isForbidden;
		if (mLbForbidden.text == "Build")
		{
			ServerAdministrator.RequestBuildLock(_mUserAdmin.Id);
		}
		else
		{
			ServerAdministrator.RequestBuildUnLock(_mUserAdmin.Id);
		}
		if (this.e_ItemAdminOnClick != null)
		{
			this.e_ItemAdminOnClick(this, _mUserAdmin);
		}
	}

	private void OnpitchBtn()
	{
		Ispitch = !Ispitch;
		mAdminstratorPitchBg.SetActive(Ispitch);
		if (this.e_ItemAdminOnpitch != null)
		{
			this.e_ItemAdminOnpitch(this, _mUserAdmin, Ispitch);
		}
	}

	private void AdminstratorOver()
	{
		mAdminstratorBg.SetActive(value: true);
	}

	private void AdminstratorOut()
	{
		mAdminstratorBg.SetActive(value: false);
	}
}
