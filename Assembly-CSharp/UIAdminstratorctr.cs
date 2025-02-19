using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAdminstratorctr : MonoBehaviour
{
	public static List<UserAdmin> mUIPersonelInfoList = new List<UserAdmin>();

	public static List<UserAdmin> mUIBalckInfoList = new List<UserAdmin>();

	public static ArrayList UIArrayPersonnelAdmin = new ArrayList();

	public static ArrayList UIArrayBlackAdmin = new ArrayList();

	public static UserAdmin _mUserAdmin = null;

	public static UserAdmin _mSelfAdmin = null;

	private void Start()
	{
	}

	public static void UpdatamPersonel()
	{
		UIArrayPersonnelAdmin.Clear();
		foreach (UserAdmin userAdmin in ServerAdministrator.UserAdminList)
		{
			if (!ServerAdministrator.IsBlack(userAdmin.Id))
			{
				UIArrayPersonnelAdmin.Add(userAdmin);
			}
			else
			{
				UIArrayBlackAdmin.Add(userAdmin);
			}
		}
	}

	public static void FobidenAll(bool Lock)
	{
		for (int i = 0; i < UIArrayPersonnelAdmin.Count; i++)
		{
			UserAdmin userAdmin = (UserAdmin)UIArrayPersonnelAdmin[i];
			if (Lock)
			{
				ServerAdministrator.RequestBuildLock(userAdmin.Id);
			}
			else
			{
				ServerAdministrator.RequestBuildUnLock(userAdmin.Id);
			}
		}
	}

	public static void ChangeAssistant(UserAdmin player)
	{
		ArrayList uIArrayPersonnelAdmin = UIArrayPersonnelAdmin;
		for (int i = 0; i < uIArrayPersonnelAdmin.Count; i++)
		{
			if (uIArrayPersonnelAdmin[i] == player)
			{
				if (player.HasPrivileges(AdminMask.BlackRole))
				{
					UIArrayPersonnelAdmin.Remove(uIArrayPersonnelAdmin[i]);
					UIArrayBlackAdmin.Add(player);
				}
				else
				{
					UIArrayPersonnelAdmin[i] = player;
				}
				break;
			}
		}
	}

	public static void ShowAssistant(UIAdminstratorItem item)
	{
		if (!(null == item))
		{
			item.PrivilegesShow(ServerAdministrator.IsAssistant(item.mUserAdmin.Id), !ServerAdministrator.IsAssistant(item.mUserAdmin.Id));
			item.BuildShow(item.mUserAdmin.HasPrivileges(AdminMask.BuildLock));
		}
	}

	public static void AddUIAssistant(UserAdmin player)
	{
		if (player != null)
		{
			UserAdmin userAdmin = ServerAdministrator.UserAdminList.Find((UserAdmin iter) => iter.Id == player.Id);
			if (userAdmin == null)
			{
				userAdmin = new UserAdmin(player.Id, player.RoleName, 0uL);
				ServerAdministrator.UserAdminList.Add(userAdmin);
			}
			userAdmin.AddPrivileges(AdminMask.AssistRole);
		}
	}

	public static void DeleteUIAssistant(UserAdmin player)
	{
		ServerAdministrator.DeleteAssistant(player.Id);
	}

	public static void UIBuildLock(UserAdmin player)
	{
		if (player == null)
		{
			return;
		}
		if (ServerAdministrator.IsAdmin(player.Id))
		{
			if (_mUserAdmin == null)
			{
				_mUserAdmin = player;
			}
			return;
		}
		if (PlayerNetwork.mainPlayerId == player.Id)
		{
			if (_mSelfAdmin == null)
			{
				_mSelfAdmin = player;
			}
			return;
		}
		UserAdmin userAdmin = ServerAdministrator.UserAdminList.Find((UserAdmin iter) => iter.Id == player.Id);
		if (userAdmin == null)
		{
			userAdmin = new UserAdmin(player.Id, player.RoleName, 0uL);
			ServerAdministrator.UserAdminList.Add(userAdmin);
		}
		userAdmin.AddPrivileges(AdminMask.BuildLock);
	}

	public static void UIAddAllBlacklist(UserAdmin Player)
	{
		if (Player == null)
		{
			return;
		}
		if (ServerAdministrator.IsAdmin(Player.Id))
		{
			if (_mUserAdmin == null)
			{
				_mUserAdmin = Player;
			}
		}
		else if (PlayerNetwork.mainPlayerId == Player.Id)
		{
			if (_mSelfAdmin == null)
			{
				_mSelfAdmin = Player;
			}
		}
		else
		{
			UIArrayBlackAdmin.Add(Player);
			mUIBalckInfoList.Add(Player);
			UIAddBlacklist(Player);
		}
	}

	public static void UIAddBlacklist(UserAdmin Player)
	{
		if (Player == null)
		{
			return;
		}
		if (ServerAdministrator.IsAdmin(Player.Id))
		{
			if (_mUserAdmin == null)
			{
				_mUserAdmin = Player;
			}
		}
		else if (PlayerNetwork.mainPlayerId == Player.Id)
		{
			if (_mSelfAdmin == null)
			{
				_mSelfAdmin = Player;
			}
		}
		else
		{
			ServerAdministrator.RequestAddBlackList(Player.Id);
		}
	}
}
