using System.IO;
using Pathea.PeEntityExt;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class EntityInfoCmpt : PeCmpt, IPeMsg
{
	public const int VERSION_0000 = 0;

	public const int CURRENT_VERSION = 0;

	public const string OverHeadPrefabPath = "Prefab/Npc/Component/OverHead";

	private CommonCmpt mCommon;

	private NpcOverHead mHeadInfo;

	private SkAliveEntity mAliveEntity;

	private CharacterName mCharacterName;

	private Texture mFaceTex;

	private string mFaceIcon;

	private string mFaceIconBig;

	private string mShopIcon;

	private NpcMissionState mMissionState = NpcMissionState.Max;

	private int mMapIcon = 21;

	public static GameObject _overheadTmplGo;

	private bool bShowrevival;

	private float m_startTime;

	private float m_delaytime;

	private float m_endtime;

	private bool SetTime;

	public NpcOverHead OverHead => mHeadInfo;

	public Texture faceTex
	{
		get
		{
			if (mFaceTex == null)
			{
				mFaceTex = TakePhoto();
			}
			return mFaceTex;
		}
		set
		{
			mFaceTex = value;
		}
	}

	public string faceIcon
	{
		get
		{
			return mFaceIcon;
		}
		set
		{
			mFaceIcon = value;
		}
	}

	public string faceIconBig
	{
		get
		{
			if (string.IsNullOrEmpty(mFaceIconBig))
			{
				return string.Empty;
			}
			return mFaceIconBig;
		}
		set
		{
			mFaceIconBig = value;
		}
	}

	public CharacterName characterName
	{
		get
		{
			return (mCharacterName != null) ? mCharacterName : CharacterName.Default;
		}
		set
		{
			mCharacterName = value;
			if (mHeadInfo != null)
			{
				mHeadInfo.SetNpcShowName(characterName.givenName);
			}
			SyncObjectName();
		}
	}

	public string shopIcon
	{
		get
		{
			return mShopIcon;
		}
		set
		{
			mShopIcon = value;
		}
	}

	public NpcMissionState MissionState => mMissionState;

	public int mapIcon
	{
		get
		{
			return mMapIcon;
		}
		set
		{
			mMapIcon = value;
		}
	}

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Model_Build:
		{
			GameObject gameObject = args[0] as GameObject;
			if (null != gameObject)
			{
				mHeadInfo = LoadHeadInfo(gameObject.transform.parent);
				InitOverHead();
			}
			break;
		}
		}
	}

	private Transform GetOverHeadParentTrans()
	{
		return base.transform;
	}

	private NpcOverHead LoadHeadInfo(Transform parentTrans)
	{
		if (null == parentTrans)
		{
			return null;
		}
		if (_overheadTmplGo == null)
		{
			_overheadTmplGo = Resources.Load("Prefab/Npc/Component/OverHead") as GameObject;
			if (null == _overheadTmplGo)
			{
				Debug.LogError("Load [Prefab/Npc/Component/OverHead] error.");
				return null;
			}
		}
		GameObject gameObject = Object.Instantiate(_overheadTmplGo);
		if (null == gameObject)
		{
			Debug.LogError("Load [Prefab/Npc/Component/OverHead] error.");
			return null;
		}
		gameObject.transform.parent = parentTrans;
		return gameObject.GetComponent<NpcOverHead>();
	}

	private bool GetNameColor()
	{
		return true;
	}

	private void SyncObjectName()
	{
		GameObject gameObject = base.Entity.GetGameObject();
		if (null != gameObject)
		{
			gameObject.name = characterName.givenName + "_" + base.Entity.Id;
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
		mCommon = base.Entity.GetCmpt<CommonCmpt>();
		mAliveEntity = base.Entity.GetCmpt<SkAliveEntity>();
		if (mAliveEntity != null)
		{
			mAliveEntity.deathEvent += OndeathEnvent;
		}
		SyncObjectName();
		SetVisiable(flag: true);
	}

	private void OndeathEnvent(SkEntity self, SkEntity carster)
	{
		bShowrevival = true;
	}

	private void OnHpChange(SkEntity caster, float hpChange)
	{
		if (mHeadInfo != null)
		{
			mHeadInfo.HpChange(caster, hpChange);
		}
	}

	public override void OnUpdate()
	{
		Updatarevival();
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num > 0)
		{
			Debug.LogError("version error");
			return;
		}
		byte[] array = PETools.Serialize.ReadBytes(r);
		if (array != null)
		{
			mCharacterName = new CharacterName();
			mCharacterName.Import(array);
		}
		mFaceIcon = PETools.Serialize.ReadNullableString(r);
		mFaceIconBig = PETools.Serialize.ReadNullableString(r);
		mShopIcon = PETools.Serialize.ReadNullableString(r);
		mMissionState = (NpcMissionState)r.ReadInt32();
		mMapIcon = r.ReadInt32();
		Invoke("RefreshState", 2f);
	}

	private void RefreshState()
	{
		if (!(mCommon.Entity.GetUserData() is NpcMissionData))
		{
			mMissionState = NpcMissionState.Max;
		}
		else
		{
			MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(mCommon.Entity);
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		byte[] buff = null;
		if (mCharacterName != null)
		{
			buff = mCharacterName.Export();
		}
		PETools.Serialize.WriteBytes(buff, w);
		PETools.Serialize.WriteNullableString(w, mFaceIcon);
		PETools.Serialize.WriteNullableString(w, mFaceIconBig);
		PETools.Serialize.WriteNullableString(w, mShopIcon);
		w.Write((int)mMissionState);
		w.Write(mMapIcon);
	}

	private Texture TakePhoto()
	{
		BiologyViewCmpt biologyViewCmpt = base.Entity.biologyViewCmpt;
		if (biologyViewCmpt == null)
		{
			return null;
		}
		CommonCmpt commonCmpt = base.Entity.commonCmpt;
		if (commonCmpt == null)
		{
			return null;
		}
		return PeViewStudio.TakePhoto(biologyViewCmpt, 64, 64, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
	}

	public void SetVisiable(bool flag)
	{
		if (!(null == mHeadInfo))
		{
			mHeadInfo.Visiable = flag;
			if (flag)
			{
				SyncInfo();
			}
			else
			{
				mHeadInfo.Reset();
			}
		}
	}

	private void InitOverHead()
	{
		if (!(mHeadInfo == null))
		{
			mAliveEntity.onHpChange += OnHpChange;
			mHeadInfo.SetTheEntity(base.Entity);
			mHeadInfo.InitTheentity(base.Entity);
			if (mCommon != null && mCommon.entityProto != null && mCommon.entityProto.proto != EEntityProto.Npc && mCommon.entityProto.proto != EEntityProto.RandomNpc)
			{
				mMissionState = NpcMissionState.Max;
			}
			SyncInfo();
		}
	}

	public void SyncInfo()
	{
		if (!(null == mHeadInfo) && !(mCommon == null))
		{
			if (mCommon.entityProto != null)
			{
				mHeadInfo.SetNpcShowName(characterName.OverHeadName);
				mHeadInfo.SetProto(mCommon.entityProto.proto);
				mHeadInfo.CurEEntityProto = mCommon.entityProto.proto;
			}
			mHeadInfo.SetNpcShopIcon(shopIcon);
			mHeadInfo.SetNameColord(GetNameColor());
			mHeadInfo.SetState(MissionState);
			if (base.Entity.entityProto.proto == EEntityProto.RandomNpc)
			{
				string storeNpcIcon = StoreRepository.GetStoreNpcIcon(base.Entity.entityProto.protoId);
				mHeadInfo.SetShowIcon(storeNpcIcon);
			}
			else
			{
				string storeNpcIcon2 = StoreRepository.GetStoreNpcIcon(base.Entity.Id);
				mHeadInfo.SetShowIcon(storeNpcIcon2);
			}
		}
	}

	public void SetDelaytime(float startTime, float delaytime)
	{
		m_startTime = startTime;
		m_delaytime = delaytime;
		m_endtime = startTime + delaytime;
		SetTime = true;
	}

	private void Updatarevival()
	{
		if (bShowrevival && SetTime)
		{
			float num = 1f - (m_endtime - Time.time) / m_delaytime;
			if (num > 1f)
			{
				bShowrevival = false;
				SetTime = false;
			}
			SetRevivalMark(bShowrevival, num);
		}
	}

	public void ShowName(bool show)
	{
		if (mHeadInfo != null)
		{
			mHeadInfo.NameLbShow(show);
		}
	}

	public void ShowBlood(bool show)
	{
		if (mHeadInfo != null)
		{
			mHeadInfo.BloodShow(show);
		}
	}

	public void ShowMissionMark(bool show)
	{
		if (mHeadInfo != null)
		{
			mHeadInfo.ShowMissionMark(show);
		}
	}

	public void SetMissionState(NpcMissionState state)
	{
		mMissionState = state;
		SyncInfo();
	}

	public void SetRevivalMark(bool show, float percent)
	{
		if (null != mHeadInfo)
		{
			mHeadInfo.SetRevivalMark(mCommon.entityProto.proto, show, percent);
			mHeadInfo.SetNameShow(!show);
		}
	}

	public void NpcSayOneWord(int _contentId, float _interval, ENpcSpeakType _type)
	{
		Texture iconTex = faceTex;
		string text = string.Empty;
		string empty = string.Empty;
		TalkData talkData = TalkRespository.GetTalkData(_contentId);
		empty = characterName.fullName + ":";
		if (talkData != null)
		{
			text = talkData.m_Content;
			text = text.Replace("\"name%\"", PeSingleton<PeCreature>.Instance.mainPlayer.ToString());
		}
		switch (_type)
		{
		case ENpcSpeakType.Topleft:
			new PeTipMsg(empty + text, iconTex, PeTipMsg.EMsgLevel.Norm);
			break;
		case ENpcSpeakType.TopHead:
			if (mHeadInfo != null)
			{
				mHeadInfo.SayOneWord(text, _interval);
			}
			break;
		case ENpcSpeakType.Both:
			new PeTipMsg(empty + text, iconTex, PeTipMsg.EMsgLevel.Norm);
			if (mHeadInfo != null)
			{
				mHeadInfo.SayOneWord(text, _interval);
			}
			break;
		}
	}
}
