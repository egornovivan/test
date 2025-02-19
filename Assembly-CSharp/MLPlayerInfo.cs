using System.Collections.Generic;
using CustomData;
using UnityEngine;

public class MLPlayerInfo : MonoBehaviour
{
	public int[] equipID_Male = new int[0];

	public int[] equipID_FeMale = new int[0];

	public RolesControl mRolesCtrl;

	private static MLPlayerInfo self = null;

	private static string selectedRoleName;

	private static int MaxRoleCount = 3;

	private static List<MLRoleInfo> mRoleInfoList = new List<MLRoleInfo>();

	public static MLPlayerInfo Instance => self;

	public int GetMaxRoleCount()
	{
		return MaxRoleCount;
	}

	public bool DeleteRole(int roleId)
	{
		int listIndex = -1;
		if (mRoleInfoList.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < mRoleInfoList.Count; i++)
		{
			if (mRoleInfoList[i] != null && mRoleInfoList[i].mRoleInfo.roleID == roleId)
			{
				listIndex = i;
				break;
			}
		}
		return deleteRoleInfo(listIndex);
	}

	public void SetSelectedRole(string roleName)
	{
		selectedRoleName = roleName;
	}

	public void UpdateScene()
	{
		if (!(mRolesCtrl == null))
		{
			UpdateRolesInfo();
			mRolesCtrl.UpdateModeInfo();
		}
	}

	public int GetRoleListNotNullCount()
	{
		int num = 0;
		for (int i = 0; i < mRoleInfoList.Count; i++)
		{
			if (mRoleInfoList[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public MLRoleInfo GetRoleInfo(int index)
	{
		if (index >= MaxRoleCount)
		{
			return null;
		}
		if (mRoleInfoList.Count == 0)
		{
			return null;
		}
		return mRoleInfoList[index];
	}

	private void Awake()
	{
		self = this;
		UpdateRolesInfo();
	}

	public void UpdateRolesInfo()
	{
		mRoleInfoList.Clear();
		if (GameClientLobby.Self != null)
		{
			for (int i = 0; i < GameClientLobby.Self.myRolesExisted.Count; i++)
			{
				if (GameClientLobby.Self.myRolesExisted[i].deletedFlag != 1)
				{
					AddRoleInfo(GameClientLobby.Self.myRolesExisted[i]);
				}
			}
		}
		if (selectedRoleName == null)
		{
			return;
		}
		int selectedIndex = -1;
		if (mRoleInfoList.Count == 0)
		{
			return;
		}
		for (int j = 0; j < mRoleInfoList.Count; j++)
		{
			if (mRoleInfoList[j] != null && mRoleInfoList[j].name == selectedRoleName)
			{
				selectedIndex = j;
				break;
			}
		}
		mRolesCtrl.SetSelectedIndex(selectedIndex);
	}

	public void Destory()
	{
		mRoleInfoList.Clear();
	}

	private void AddRoleInfo(RoleInfo info)
	{
		int num = -1;
		if (mRoleInfoList.Count == 0)
		{
			for (int i = 0; i < MaxRoleCount; i++)
			{
				mRoleInfoList.Add(null);
			}
		}
		for (int j = 0; j < MaxRoleCount; j++)
		{
			if (mRoleInfoList[j] == null)
			{
				num = j;
				break;
			}
		}
		if (num != -1)
		{
			MLRoleInfo mLRoleInfo = new MLRoleInfo();
			mLRoleInfo.mRoleInfo = info;
			mLRoleInfo.name = info.name;
			mLRoleInfo.sex = info.sex;
			mRoleInfoList[num] = mLRoleInfo;
		}
	}

	private bool deleteRoleInfo(int ListIndex)
	{
		if (ListIndex == -1)
		{
			return false;
		}
		if (ListIndex >= mRoleInfoList.Count)
		{
			return false;
		}
		mRoleInfoList[ListIndex] = null;
		mRolesCtrl.DeleteModeInfo(ListIndex);
		return true;
	}
}
