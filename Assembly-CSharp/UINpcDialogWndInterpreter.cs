using System;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PeCustom;
using UnityEngine;

public class UINpcDialogWndInterpreter : MonoBehaviour
{
	public delegate void DNotify(int world_index, int npo_id, int quest_id);

	public UINpcDialogWnd npcDialogWnd;

	public bool refreshQuestsNow;

	private bool _initialized;

	private IList<string> _dialogs;

	public int npoId { get; private set; }

	public PeEntity npoEntity { get; private set; }

	public event DNotify onQuestClick;

	public bool SetNpoEntity(PeEntity npo_entity)
	{
		PeScenarioEntity component = npo_entity.gameObject.GetComponent<PeScenarioEntity>();
		if (component != null)
		{
			npoId = component.spawnPoint.ID;
			npoEntity = npo_entity;
			npcDialogWnd.SetNPCInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIcon());
			refreshQuestsNow = true;
			return true;
		}
		return false;
	}

	public bool SetNpoId(int npo_id)
	{
		SpawnDataSource spawnData = PeCustomScene.Self.spawnData;
		if (spawnData.ContainMonster(npo_id))
		{
			MonsterSpawnPoint monster = spawnData.GetMonster(npo_id);
			if (monster.agent != null && monster.agent.entity != null)
			{
				npoEntity = monster.agent.entity;
			}
		}
		else if (spawnData.ContainNpc(npo_id))
		{
			NPCSpawnPoint npc = spawnData.GetNpc(npo_id);
			if (npc.agent != null && npc.agent.entity != null)
			{
				npoEntity = npc.agent.entity;
			}
		}
		if (npoEntity != null)
		{
			npoId = npo_id;
			npcDialogWnd.SetNPCInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIcon());
			refreshQuestsNow = true;
			return true;
		}
		npoId = -1;
		npcDialogWnd.SetNPCInfo(string.Empty, "Null");
		return false;
	}

	public void Init()
	{
		if (!_initialized)
		{
			UINpcDialogWnd uINpcDialogWnd = npcDialogWnd;
			uINpcDialogWnd.onQuestItemClick = (Action<UINpcQuestItem>)Delegate.Combine(uINpcDialogWnd.onQuestItemClick, new Action<UINpcQuestItem>(OnQuestItemClick));
			UINpcDialogWnd uINpcDialogWnd2 = npcDialogWnd;
			uINpcDialogWnd2.onSetItemContent = (Action<UINpcQuestItem>)Delegate.Combine(uINpcDialogWnd2.onSetItemContent, new Action<UINpcQuestItem>(OnSetQuestItemContent));
			UINpcDialogWnd uINpcDialogWnd3 = npcDialogWnd;
			uINpcDialogWnd3.onBeforeShow = (Action)Delegate.Combine(uINpcDialogWnd3.onBeforeShow, new Action(OnBeforeNpcWndShow));
			UINpcDialogWnd uINpcDialogWnd4 = npcDialogWnd;
			uINpcDialogWnd4.e_OnHide = (UIBaseWidget.WndEvent)Delegate.Combine(uINpcDialogWnd4.e_OnHide, new UIBaseWidget.WndEvent(OnNpoWndHide));
			DialogMgr dialogMgr = PeCustomScene.Self.scenario.dialogMgr;
			dialogMgr.onNpoQuestsChanged = (Action<int, int>)Delegate.Combine(dialogMgr.onNpoQuestsChanged, new Action<int, int>(OnNpoQuestsChanged));
			_initialized = true;
		}
	}

	public void Close()
	{
		if (_initialized)
		{
			UINpcDialogWnd uINpcDialogWnd = npcDialogWnd;
			uINpcDialogWnd.onQuestItemClick = (Action<UINpcQuestItem>)Delegate.Remove(uINpcDialogWnd.onQuestItemClick, new Action<UINpcQuestItem>(OnQuestItemClick));
			UINpcDialogWnd uINpcDialogWnd2 = npcDialogWnd;
			uINpcDialogWnd2.onSetItemContent = (Action<UINpcQuestItem>)Delegate.Remove(uINpcDialogWnd2.onSetItemContent, new Action<UINpcQuestItem>(OnSetQuestItemContent));
			UINpcDialogWnd uINpcDialogWnd3 = npcDialogWnd;
			uINpcDialogWnd3.onBeforeShow = (Action)Delegate.Remove(uINpcDialogWnd3.onBeforeShow, new Action(OnBeforeNpcWndShow));
			UINpcDialogWnd uINpcDialogWnd4 = npcDialogWnd;
			uINpcDialogWnd4.e_OnHide = (UIBaseWidget.WndEvent)Delegate.Remove(uINpcDialogWnd4.e_OnHide, new UIBaseWidget.WndEvent(OnNpoWndHide));
			DialogMgr dialogMgr = PeCustomScene.Self.scenario.dialogMgr;
			dialogMgr.onNpoQuestsChanged = (Action<int, int>)Delegate.Remove(dialogMgr.onNpoQuestsChanged, new Action<int, int>(OnNpoQuestsChanged));
			_initialized = false;
		}
	}

	private void Update()
	{
		if (PeSingleton<CreatureMgr>.Instance != null && npcDialogWnd.isShow && npoEntity != null)
		{
			float num = Vector3.SqrMagnitude(PeSingleton<CreatureMgr>.Instance.mainPlayer.peTrans.position - npoEntity.ExtGetPos());
			if (num >= 64f)
			{
				npcDialogWnd.Hide();
			}
		}
	}

	private void LateUpdate()
	{
		if (_initialized && refreshQuestsNow)
		{
			RefreshQuests();
			refreshQuestsNow = false;
		}
	}

	public void RefreshQuests()
	{
		if (_initialized)
		{
			_dialogs = PeCustomScene.Self.scenario.dialogMgr.GetQuests(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, npoId);
			if (_dialogs != null)
			{
				npcDialogWnd.UpdateTable(_dialogs.Count);
			}
			else
			{
				npcDialogWnd.UpdateTable(0);
			}
		}
	}

	private void OnBeforeNpcWndShow()
	{
		if (refreshQuestsNow)
		{
			RefreshQuests();
			refreshQuestsNow = false;
		}
		PeScenarioUtility.SetNpoReqDialogue(npoEntity, string.Empty);
	}

	private void OnNpoWndHide(UIBaseWidget widget = null)
	{
		PeScenarioUtility.RemoveNpoReq(npoEntity, EReqType.Dialogue);
	}

	private void OnNpoQuestsChanged(int world_index, int npo_id)
	{
		if (world_index == PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex && npoId == npo_id)
		{
			refreshQuestsNow = true;
		}
	}

	private void OnSetQuestItemContent(UINpcQuestItem item)
	{
		if (_dialogs == null)
		{
			Debug.LogWarning("The giving dialog is null");
		}
		else if (item.index < 0 || item.index >= _dialogs.Count)
		{
			Debug.LogWarning("The index is out of range");
		}
		else
		{
			item.test = _dialogs[item.index];
		}
	}

	private void OnQuestItemClick(UINpcQuestItem item)
	{
		if (item.index < 0 || item.index >= _dialogs.Count)
		{
			Debug.LogWarning("The index is out of range");
			return;
		}
		int worldIndex = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex;
		int questId = PeCustomScene.Self.scenario.dialogMgr.GetQuestId(worldIndex, npoId, item.index);
		if (questId != -1)
		{
			if (this.onQuestClick != null)
			{
				this.onQuestClick(worldIndex, npoId, questId);
			}
			Debug.Log("Click the Quest id [" + questId + "]");
		}
		else
		{
			Debug.LogWarning("cant find the quest id");
		}
	}
}
