using Pathea;
using PeMap;
using UnityEngine;

public class UIMapLabel : MonoBehaviour
{
	public delegate void OnMouseOverEvent(UIMapLabel sender, bool isOver);

	public delegate void OnClickEvent(UIMapLabel sender);

	private const int m_MinRadius = 30;

	private const int m_MaxRadius = 150;

	[SerializeField]
	private UISprite mSpr;

	[SerializeField]
	private UIButton mButton;

	[SerializeField]
	private UISprite mFriendSpr;

	[SerializeField]
	private BoxCollider mBoxCollider;

	[SerializeField]
	private float m_WorldMapConvert;

	private ILabel m_Label;

	private bool inMinMap;

	private int m_NpcID;

	private Color m_Col;

	public static string[] FriendlyLevelIconName = new string[3] { "FriendlyLevel_bad", "FriendlyLevel_nomal", "FriendlyLevel_good" };

	public ILabel _ILabel => m_Label;

	public ELabelType type => m_Label.GetType();

	public bool fastTrval => m_Label.FastTravel();

	public Vector3 worldPos => m_Label.GetPos();

	public string descText => m_Label.GetText();

	public int NpcID => m_NpcID;

	public string iconStr
	{
		get
		{
			MapIcon mapIcon = PeSingleton<MapIcon.Mgr>.Instance.iconList.Find((MapIcon itr) => itr.id == m_Label.GetIcon());
			return (mapIcon == null) ? string.Empty : mapIcon.iconName;
		}
	}

	public EShow eShow => m_Label.GetShow();

	public bool Openable
	{
		get
		{
			if (m_Label == null)
			{
				return false;
			}
			return type != ELabelType.Npc && type != ELabelType.Vehicle && type != ELabelType.Mark;
		}
	}

	public event OnMouseOverEvent e_OnMouseOver;

	public event OnClickEvent e_OnClick;

	public void Init()
	{
		this.e_OnMouseOver = null;
		this.e_OnClick = null;
		m_WorldMapConvert = 1f;
		m_Label = null;
		inMinMap = false;
		m_NpcID = -1;
		SetColor(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		SetEnableClick(enable: true);
		SetFriendlyLevelIcon(-1);
		mSpr.depth = 0;
		base.transform.localRotation = Quaternion.identity;
	}

	public void SetLabel(ILabel _label, bool _inMinMap = false)
	{
		Init();
		m_Label = _label;
		inMinMap = _inMinMap;
		if (m_Label == null)
		{
			return;
		}
		UpdateIcon();
		if (_label is TownLabel)
		{
			if (PeGameMgr.IsAdventure)
			{
				TownLabel townLabel = _label as TownLabel;
				int allianceColor = townLabel.GetAllianceColor();
				if (allianceColor >= 0 && allianceColor < AllyColor.AllianceCols.Length)
				{
					Color32 color = AllyColor.AllianceCols[allianceColor];
					SetColor(color);
				}
				SetFriendlyLevelIcon(townLabel.GetFriendlyLevel());
			}
		}
		else if (_label is MissionLabel)
		{
			MissionLabel missionLabel = _label as MissionLabel;
			SetColor(missionLabel.GetMissionColor());
		}
		SetEnableClick(enable: true);
		UpdateRadiusSize();
		switch (_label.GetType())
		{
		case ELabelType.FastTravel:
			mSpr.depth = 1;
			break;
		case ELabelType.Mark:
			mSpr.depth = 2;
			break;
		case ELabelType.Vehicle:
			mSpr.depth = 3;
			break;
		case ELabelType.Revive:
			mSpr.depth = 4;
			break;
		case ELabelType.Npc:
			mSpr.depth = 5;
			break;
		case ELabelType.Mission:
		{
			MissionLabel missionLabel2 = _label as MissionLabel;
			mSpr.depth = ((!MissionManager.IsMainMission(missionLabel2.m_missionID)) ? 6 : 7);
			break;
		}
		case ELabelType.User:
			mSpr.depth = 8;
			break;
		default:
			mSpr.depth = 0;
			break;
		}
	}

	public void UpdateIcon(string iconName = null)
	{
		mSpr.spriteName = ((iconName != null) ? iconName : iconStr);
		mBoxCollider.size = new Vector3(mSpr.transform.localScale.x, mSpr.transform.localScale.y, mBoxCollider.size.z);
	}

	public void SetColor(Color32 color32)
	{
		m_Col = new Color((float)(int)color32.r / 255f, (float)(int)color32.g / 255f, (float)(int)color32.b / 255f, (float)(int)color32.a / 255f);
		mSpr.color = m_Col;
	}

	public void SetFriendlyLevelIcon(int iconIndex)
	{
		if (iconIndex < 0 || iconIndex > FriendlyLevelIconName.Length)
		{
			mFriendSpr.enabled = false;
			return;
		}
		mFriendSpr.enabled = true;
		mFriendSpr.spriteName = FriendlyLevelIconName[iconIndex];
		mFriendSpr.MakePixelPerfect();
	}

	public void SetLabelPosByNPC(int npcid)
	{
		m_NpcID = npcid;
	}

	private void UpdateLabelPosByNPC()
	{
		if (m_NpcID != -1)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_NpcID);
			if (!(peEntity == null) && (peEntity.proto == EEntityProto.Npc || peEntity.proto == EEntityProto.RandomNpc) && m_Label is MissionLabel missionLabel)
			{
				missionLabel.SetLabelPos(peEntity.position);
			}
		}
	}

	private void Update()
	{
		if (m_Label != null && inMinMap && GameUI.Instance.mMainPlayer != null)
		{
			UpdatePosInMinMap();
		}
		UpdateLabelPosByNPC();
		UpdateMisLabel();
		UpdateReviveLabel();
	}

	private void UpdateRadiusSize()
	{
		if (_ILabel.GetRadius() != -1f)
		{
			mSpr.transform.localScale = GetDiameterSize(_ILabel.GetRadius());
		}
		else
		{
			mSpr.MakePixelPerfect();
		}
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		component.size = new Vector3(mSpr.transform.localScale.x, mSpr.transform.localScale.y, component.size.z);
	}

	private Vector3 GetDiameterSize(float radius)
	{
		float num = Mathf.Clamp(radius, 30f, 150f);
		return new Vector3(num, num, 1f);
	}

	private float ConvetMToPx(float m)
	{
		float num = 0f;
		float num2 = 0f;
		if (PeGameMgr.IsAdventure || PeGameMgr.IsBuild)
		{
			if (RandomMapConfig.Instance != null)
			{
				num = 3140f;
				num2 = RandomMapConfig.Instance.MapSize.x;
			}
		}
		else
		{
			num = 4096f;
			num2 = 18432f;
		}
		return num / num2 * m;
	}

	private void UpdatePosInMinMap()
	{
		Vector3 vector = worldPos - GameUI.Instance.mMainPlayer.position;
		vector *= m_WorldMapConvert;
		float x = vector.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x;
		float y = vector.z * GameUI.Instance.mUIMinMapCtrl.mMapScale.y;
		base.transform.localPosition = new Vector3(x, y, 0f);
	}

	private void OnHover(bool isOver)
	{
		if (this.e_OnMouseOver != null && !inMinMap)
		{
			this.e_OnMouseOver(this, isOver);
		}
	}

	private void OnClick()
	{
		if (this.e_OnClick != null && !inMinMap)
		{
			this.e_OnClick(this);
		}
	}

	private void UpdateMisLabel()
	{
		if (m_Label == null || m_Label.GetType() != ELabelType.Mission || !(m_Label is MissionLabel missionLabel))
		{
			return;
		}
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(missionLabel.m_missionID);
		if (missionLabel.m_type == MissionLabelType.misLb_target && missionLabel.m_target != null)
		{
			if (missionLabel.m_target.mComplete)
			{
				PeSingleton<LabelMgr>.Instance.Remove(_ILabel);
			}
			else if (missionView != null)
			{
				mSpr.enabled = missionView.mMissionTag;
			}
		}
		else if (missionLabel.m_type == MissionLabelType.misLb_end && missionView != null && missionView.mComplete != missionLabel.IsComplete)
		{
			missionLabel.IsComplete = missionView.mComplete;
			SetColor(missionLabel.GetMissionColor());
			SetEnableClick(enable: true);
		}
	}

	private void UpdateReviveLabel()
	{
		if (m_Label != null && m_Label.GetType() == ELabelType.Revive && m_Label is ReviveLabel reviveLabel && Vector3.Distance(reviveLabel.GetPos(), PeSingleton<PeCreature>.Instance.mainPlayer.position) < 50f)
		{
			PeSingleton<ReviveLabel.Mgr>.Instance.Remove(reviveLabel);
		}
	}

	private void SetEnableClick(bool enable)
	{
		if (enable)
		{
			mButton.defaultColor = m_Col;
			mButton.hover = new Color(m_Col.r, m_Col.g, m_Col.b, m_Col.a * 0.5f);
			mButton.disabledColor = new Color(m_Col.r * 0.6f, m_Col.g * 0.6f, m_Col.b * 0.6f, m_Col.a);
			mButton.pressed = new Color(m_Col.r * 0.6f, m_Col.g * 0.6f, m_Col.b * 0.6f, m_Col.a);
		}
		else
		{
			mButton.defaultColor = m_Col;
			mButton.hover = m_Col;
			mButton.disabledColor = m_Col;
			mButton.pressed = m_Col;
		}
		mButton.isEnabled = enable;
	}
}
