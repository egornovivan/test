using Pathea;
using UnityEngine;

public class MissionSelItem_N : MonoBehaviour
{
	public UILabel mMissionContent;

	private UINPCTalk mParentNPCTalk;

	private UINpcWnd mNPCGui;

	public int mMissionId;

	public UISprite mMissionMarke;

	public PeEntity m_Player;

	public void SetMission(int missionId, UIBaseWidget parent)
	{
		mMissionId = missionId;
		mParentNPCTalk = parent as UINPCTalk;
		mNPCGui = parent as UINpcWnd;
		mMissionContent.text = MissionRepository.GetMissionNpcListName(mMissionId, bspe: false);
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(missionId);
		if (missionCommonData == null)
		{
			return;
		}
		if (mMissionMarke != null)
		{
			if (m_Player != null)
			{
				if (MissionManager.Instance.HasMission(missionId))
				{
					if (missionCommonData.m_Type == MissionType.MissionType_Main)
					{
						if (MissionManager.Instance.IsReplyMission(missionId))
						{
							mMissionMarke.spriteName = "MainMissionEnd";
						}
						else
						{
							mMissionMarke.spriteName = "MissionNotEnd";
						}
					}
					else if (missionCommonData.IsTalkMission())
					{
						mMissionMarke.spriteName = "SubMission";
					}
					else if (MissionManager.Instance.IsReplyMission(missionId))
					{
						mMissionMarke.spriteName = "SubMissionEnd";
					}
					else
					{
						mMissionMarke.spriteName = "MissionNotEnd";
					}
				}
				else if (missionCommonData.m_Type == MissionType.MissionType_Main)
				{
					mMissionMarke.spriteName = "MainMissionGet";
				}
				else if (missionCommonData.m_Type == MissionType.MissionType_Sub)
				{
					mMissionMarke.spriteName = "SubMissionGet";
				}
				else
				{
					mMissionMarke.spriteName = "SubMission";
				}
			}
			else
			{
				mMissionMarke.gameObject.SetActive(value: false);
			}
			mMissionMarke.MakePixelPerfect();
			return;
		}
		if (MissionManager.Instance.HasMission(missionId))
		{
			if (missionCommonData.m_Type == MissionType.MissionType_Main && !MissionManager.HasRandomMission(missionId))
			{
				if (MissionManager.Instance.IsReplyMission(missionId))
				{
					mMissionContent.color = Color.yellow;
				}
				else
				{
					mMissionContent.color = Color.white;
				}
			}
			else if (missionCommonData.IsTalkMission())
			{
				mMissionContent.color = Color.white;
			}
			else if (MissionManager.Instance.IsReplyMission(missionId))
			{
				mMissionContent.color = Color.yellow;
			}
			else
			{
				mMissionContent.color = Color.white;
			}
		}
		else
		{
			mMissionContent.color = Color.white;
		}
		UIButton component = mMissionContent.GetComponent<UIButton>();
		if ((bool)component)
		{
			component.defaultColor = mMissionContent.color;
		}
	}

	public void ActiveMask()
	{
		mMissionMarke.gameObject.SetActive(value: true);
	}

	public void SetMission(int missionId, string content, UIBaseWidget parent, PeEntity player)
	{
		m_Player = player;
		SetMission(missionId, parent);
		content = GameUI.Instance.mNPCTalk.ParseStrDefine(content, MissionManager.GetMissionCommonData(missionId));
		mMissionContent.text = content;
	}

	public void SetMission(int missionId, string content, string icon, UIBaseWidget parent, PeEntity player)
	{
		m_Player = player;
		SetMission(missionId, parent);
		mMissionContent.text = content;
		if (mMissionMarke != null)
		{
			if (icon == "Null")
			{
				mMissionMarke.enabled = false;
			}
			else
			{
				mMissionMarke.spriteName = icon;
			}
			mMissionMarke.MakePixelPerfect();
		}
	}

	private void OnBtnClick()
	{
		if (mParentNPCTalk != null)
		{
			mParentNPCTalk.OnMutexBtnClick(mMissionId, mMissionContent.text);
		}
		if (mNPCGui != null)
		{
			mNPCGui.OnMutexBtnClick(mMissionId);
		}
	}
}
