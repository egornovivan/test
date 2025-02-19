using System;
using System.Collections.Generic;
using AppearBlendShape;
using CustomCharactor;
using UnityEngine;

public class RolesControl : MonoBehaviour
{
	public Vector3 mRoleModeLocalPos_Male;

	public Vector3 mRoleModeLocalPos_Female;

	public Vector3 mRoleModeLocalScale;

	public Camera mainCamera;

	public GameObject[] Roles;

	private int selectedIndex = -1;

	private int mouseMoveIndex;

	private bool bMoseMoveIn;

	private RoleCompent[] roleCompents;

	public GameObject[] ModePrefab;

	public string[] SelectedRolesAnimatorContorl;

	public string[] MouseMoveRolesAnimatorContorl;

	private int modesCount;

	private List<ModeInfo> modeInfoList = new List<ModeInfo>();

	private AvatarData mMaleAvataData;

	private AvatarData mFemaleAvataData;

	private int index;

	private string oldAnim = string.Empty;

	private bool addAnminIndex;

	private int MouseMovePalyCount;

	public GameObject Selected => Roles[selectedIndex];

	public int GetSelectedIndex()
	{
		return selectedIndex;
	}

	public void SetSelectedIndex(int index)
	{
		selectedIndex = index;
	}

	private void Awake()
	{
		mMaleAvataData = new AvatarData();
		mFemaleAvataData = new AvatarData();
	}

	private void Start()
	{
		if (selectedIndex == -1)
		{
			selectedIndex = 0;
		}
		roleCompents = new RoleCompent[Roles.Length];
		for (int i = 0; i < Roles.Length; i++)
		{
			float f = (float)i / (float)Roles.Length * (float)Math.PI * 2f;
			Roles[i].transform.position = new Vector3(10f * Mathf.Cos(f), 0f, 10f * Mathf.Sin(f));
			roleCompents[i] = Roles[i].GetComponent<RoleCompent>();
		}
		UpdateModeInfo();
	}

	public void DeleteModeInfo(int deleteIndex)
	{
		UnityEngine.Object.Destroy(modeInfoList[deleteIndex].mMode);
		modeInfoList[deleteIndex] = null;
	}

	public void UpdateModeInfo()
	{
		RebuildModels();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			MousePickRole();
			StopAnimator();
		}
		UpdateRoles();
		MouseMoveRole();
		PalyRolesAnimator();
		if (bMoseMoveIn)
		{
			MouseMoveAnimator();
		}
	}

	private void RebuildModels()
	{
		if (modesCount != 0)
		{
			for (int i = 0; i < modeInfoList.Count; i++)
			{
				if (modeInfoList[i].mMode != null)
				{
					UnityEngine.Object.Destroy(modeInfoList[i].mMode);
				}
			}
			modeInfoList.Clear();
		}
		modesCount = MLPlayerInfo.Instance.GetMaxRoleCount();
		for (int j = 0; j < modesCount; j++)
		{
			ModeInfo modeInfo = new ModeInfo();
			modeInfo.mRoleInfo = MLPlayerInfo.Instance.GetRoleInfo(j);
			if (modeInfo.mRoleInfo == null)
			{
				modeInfo.mModel = null;
			}
			else
			{
				bool flag = modeInfo.mRoleInfo.sex == 1;
				ESex eSex = (flag ? ESex.Female : ESex.Male);
				GameObject gameObject = UnityEngine.Object.Instantiate((eSex != ESex.Female) ? ModePrefab[1] : ModePrefab[0]);
				modeInfo.mModel = gameObject.GetComponent<PlayerModel>();
				modeInfo.mModel.mAppearData = new AppearData();
				modeInfo.mModel.mAppearData.Deserialize(modeInfo.mRoleInfo.mRoleInfo.appearData);
				modeInfo.mModel.mNude = new AvatarData();
				modeInfo.mModel.mNude.Deserialize(modeInfo.mRoleInfo.mRoleInfo.nudeData);
				modeInfo.mModel.mClothed = ((!flag) ? mMaleAvataData : mFemaleAvataData);
				modeInfo.mMode.transform.parent = roleCompents[j].m_boxCollider.transform;
				modeInfo.mMode.transform.localPosition = ((!flag) ? new Vector3(mRoleModeLocalPos_Male.x, mRoleModeLocalPos_Male.y, mRoleModeLocalPos_Male.z) : new Vector3(mRoleModeLocalPos_Female.x, mRoleModeLocalPos_Female.y, mRoleModeLocalPos_Female.z));
				modeInfo.mMode.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
				modeInfo.mMode.transform.localScale = new Vector3(mRoleModeLocalScale.x, mRoleModeLocalScale.y, mRoleModeLocalScale.x);
				modeInfo.mMode.SetActive(value: true);
				modeInfo.mModel.BuildModel();
			}
			modeInfoList.Add(modeInfo);
		}
	}

	private void PalyRolesAnimator()
	{
		if (selectedIndex < 0 || selectedIndex >= modesCount || modeInfoList.Count == 0 || modeInfoList[selectedIndex] == null || modeInfoList[selectedIndex].mMode == null)
		{
			return;
		}
		int num = SelectedRolesAnimatorContorl.Length;
		if (num == 0)
		{
			return;
		}
		Animator component = modeInfoList[selectedIndex].mMode.GetComponent<Animator>();
		int num2 = Mathf.FloorToInt(component.GetCurrentAnimatorStateInfo(0).normalizedTime);
		float num3 = component.GetCurrentAnimatorStateInfo(0).normalizedTime - (float)num2;
		if (num3 > 0.95f && !addAnminIndex)
		{
			if ("DressStand" != SelectedRolesAnimatorContorl[index])
			{
				component.SetBool(SelectedRolesAnimatorContorl[index], value: true);
				oldAnim = SelectedRolesAnimatorContorl[index];
				Invoke("CloseAimn", 0.1f);
			}
			index++;
			if (index >= num)
			{
				index = 0;
			}
			addAnminIndex = true;
		}
		else if (addAnminIndex && num3 < 0.5f)
		{
			addAnminIndex = false;
		}
	}

	private void CloseAimn()
	{
		if (selectedIndex >= 0 && selectedIndex < modesCount && modeInfoList.Count != 0 && modeInfoList[selectedIndex] != null && !(modeInfoList[selectedIndex].mMode == null) && SelectedRolesAnimatorContorl.Length != 0)
		{
			Animator component = modeInfoList[selectedIndex].mMode.GetComponent<Animator>();
			component.SetBool(oldAnim, value: false);
		}
	}

	private void MouseMoveAnimator()
	{
		if (selectedIndex == mouseMoveIndex || modeInfoList[mouseMoveIndex] == null || modeInfoList[mouseMoveIndex].mMode == null)
		{
			return;
		}
		Animator component = modeInfoList[mouseMoveIndex].mMode.GetComponent<Animator>();
		int num = MouseMoveRolesAnimatorContorl.Length;
		if (num != 0)
		{
			int num2 = MouseMovePalyCount % num;
			component.SetBool(MouseMoveRolesAnimatorContorl[num2], value: true);
			if (num2 != 0)
			{
				component.SetBool(MouseMoveRolesAnimatorContorl[num2 - 1], value: false);
			}
			else
			{
				component.SetBool(MouseMoveRolesAnimatorContorl[num - 1], value: false);
			}
			MouseMovePalyCount++;
		}
	}

	private void StopAnimator()
	{
		for (int i = 0; i < modesCount; i++)
		{
			if (i != selectedIndex && modeInfoList[i] != null && !(modeInfoList[i].mMode == null))
			{
				Animator component = modeInfoList[i].mMode.GetComponent<Animator>();
				component.SetBool("Common1", value: false);
				component.SetBool("Common2", value: false);
				component.SetBool("DressIdle1", value: false);
				component.SetBool("DressIdle2", value: false);
			}
		}
	}

	private void MousePickRole()
	{
		Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		float num = 1000f;
		for (int i = 0; i < roleCompents.Length; i++)
		{
			if (roleCompents[i].m_boxCollider.Raycast(ray, out var hitInfo, 100f) && hitInfo.distance < num)
			{
				num = hitInfo.distance;
				selectedIndex = i;
			}
		}
	}

	private void UpdateRoles()
	{
		Vector3 euler = new Vector3(0f, (float)selectedIndex / (float)Roles.Length * 360f, 0f);
		Quaternion b = Quaternion.Euler(euler);
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, b, 0.05f);
		for (int i = 0; i < 3; i++)
		{
			roleCompents[i].transform.rotation = Quaternion.identity;
		}
	}

	private void MouseMoveRole()
	{
		Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		for (int i = 0; i < Roles.Length; i++)
		{
			if (selectedIndex == i)
			{
				continue;
			}
			if (roleCompents[i].m_boxCollider.Raycast(ray, out var _, 100f))
			{
				mouseMoveIndex = i;
				if (!bMoseMoveIn)
				{
					MouseMovePalyCount = 0;
					bMoseMoveIn = true;
				}
			}
			else if (i == mouseMoveIndex && bMoseMoveIn)
			{
				bMoseMoveIn = false;
				StopAnimator();
			}
		}
	}

	public void TakeRoleHerderTexture()
	{
		if (selectedIndex != -1 && selectedIndex < Roles.Length && modeInfoList[selectedIndex].mMode != null)
		{
			PeViewStudio.TakePhoto(modeInfoList[selectedIndex].mMode, 64, 64, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
		}
	}
}
