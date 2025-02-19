using UnityEngine;

public class PersonnelItem_N : MonoBehaviour
{
	public UICheckbox mBuildEnable;

	public UILabel mBuildEnableText;

	public UICheckbox mIsAssistant;

	public UILabel mIsAssistantText;

	public UILabel mName;

	public UISprite mManagerMask;

	public UICheckbox mSelectedBg;

	private int mRoleId;

	public void SetPlayerInfo(UserAdmin ud)
	{
		mName.text = ud.RoleName;
		mRoleId = ud.Id;
		if (ServerAdministrator.IsAdmin(mRoleId))
		{
			mManagerMask.spriteName = "AdministratorMask";
			mIsAssistantText.text = "Set";
			mBuildEnableText.text = "Forbidden";
			mIsAssistant.isChecked = false;
			mBuildEnable.isChecked = true;
			return;
		}
		if (ServerAdministrator.IsAssistant(mRoleId))
		{
			mManagerMask.spriteName = "AssistantMask";
			mIsAssistantText.text = "Dismiss";
		}
		else
		{
			mManagerMask.spriteName = "Null";
			mIsAssistantText.text = "Set";
		}
		if (ServerAdministrator.IsBuildLock(mRoleId))
		{
			mBuildEnableText.text = "Allow";
		}
		else
		{
			mBuildEnableText.text = "Forbidden";
		}
		mIsAssistant.isChecked = ServerAdministrator.IsAssistant(mRoleId);
		mBuildEnable.isChecked = !ServerAdministrator.IsBuildLock(mRoleId);
	}

	public void OnBan()
	{
		if (null != PlayerNetwork.mainPlayer)
		{
			ServerAdministrator.RequestAddBlackList(mRoleId);
		}
	}

	private void OnBuildEnableSelected(bool selected)
	{
		if (!(null == PlayerNetwork.mainPlayer) && ServerAdministrator.IsBuildLock(mRoleId) == selected)
		{
			if (!selected)
			{
				ServerAdministrator.RequestBuildLock(mRoleId);
			}
			else
			{
				ServerAdministrator.RequestBuildUnLock(mRoleId);
			}
		}
	}

	private void OnAssistantSelected(bool selected)
	{
		if (!(null == PlayerNetwork.mainPlayer) && ServerAdministrator.IsAssistant(mRoleId) != selected)
		{
			if (selected)
			{
				ServerAdministrator.RequestAddAssistants(mRoleId);
			}
			else
			{
				ServerAdministrator.RequestDeleteAssistants(mRoleId);
			}
		}
	}

	private void OnSelectBgSelected(bool selected)
	{
	}
}
