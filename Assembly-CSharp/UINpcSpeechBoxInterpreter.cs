using System;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using PeCustom;
using UnityEngine;

public class UINpcSpeechBoxInterpreter : MonoBehaviour
{
	public delegate void DNotifyParmInt(int choice);

	public delegate void DNotityParmVoid();

	public UINpcSpeechBox npcSpeechBox;

	private bool _initialized;

	private IList<string> _choices;

	public int npoId { get; private set; }

	public PeEntity npoEntity { get; private set; }

	public event DNotifyParmInt onChoiceClickForward;

	public event DNotifyParmInt onChoiceClick;

	public event DNotityParmVoid onUIClick;

	public void SetSpeechContent(string text)
	{
		if (_initialized)
		{
			npcSpeechBox.SetContent(text);
		}
	}

	public void SetChoiceCount()
	{
		if (_initialized)
		{
			_choices = PeCustomScene.Self.scenario.dialogMgr.GetChoices();
			npcSpeechBox.SetContent(_choices.Count);
		}
	}

	public bool SetNpoEntity(PeEntity entity)
	{
		if (!_initialized)
		{
			return false;
		}
		PeScenarioEntity component = entity.gameObject.GetComponent<PeScenarioEntity>();
		if (component != null)
		{
			npoId = component.spawnPoint.ID;
			npoEntity = entity;
			npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIconBig());
			return true;
		}
		if (entity == PeSingleton<CreatureMgr>.Instance.mainPlayer)
		{
			npoId = -1;
			npoEntity = entity;
			BiologyViewCmpt biologyViewCmpt = npoEntity.biologyViewCmpt;
			Texture2D icon = PeViewStudio.TakePhoto(biologyViewCmpt, 150, 150, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
			npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), icon);
		}
		return false;
	}

	public bool SetNpoId(int npo_id)
	{
		if (!_initialized)
		{
			return false;
		}
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
			npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIconBig());
			return true;
		}
		npoId = -1;
		npcSpeechBox.SetNpcInfo(string.Empty, "Null");
		return false;
	}

	public bool SetObject(OBJECT obj)
	{
		if (!_initialized)
		{
			return false;
		}
		npoEntity = PeScenarioUtility.GetEntity(obj);
		if (npoEntity != null)
		{
			npoId = obj.Id;
			if (npoEntity == PeSingleton<CreatureMgr>.Instance.mainPlayer)
			{
				BiologyViewCmpt biologyViewCmpt = npoEntity.biologyViewCmpt;
				Texture2D icon = PeViewStudio.TakePhoto(biologyViewCmpt, 150, 150, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
				npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), icon);
			}
			else
			{
				npcSpeechBox.SetNpcInfo(npoEntity.ExtGetName(), npoEntity.ExtGetFaceIconBig());
			}
			return true;
		}
		npoId = -1;
		npcSpeechBox.SetNpcInfo(string.Empty, "Null");
		return false;
	}

	public void Init()
	{
		if (!_initialized)
		{
			UINpcSpeechBox uINpcSpeechBox = npcSpeechBox;
			uINpcSpeechBox.onQuestItemClick = (Action<UINpcQuestItem>)Delegate.Combine(uINpcSpeechBox.onQuestItemClick, new Action<UINpcQuestItem>(OnQuestItemClick));
			UINpcSpeechBox uINpcSpeechBox2 = npcSpeechBox;
			uINpcSpeechBox2.onSetItemContent = (Action<UINpcQuestItem>)Delegate.Combine(uINpcSpeechBox2.onSetItemContent, new Action<UINpcQuestItem>(OnSetQuestItemContent));
			UINpcSpeechBox uINpcSpeechBox3 = npcSpeechBox;
			uINpcSpeechBox3.onUIClick = (Action)Delegate.Combine(uINpcSpeechBox3.onUIClick, new Action(OnUIClick));
			DialogMgr dialogMgr = PeCustomScene.Self.scenario.dialogMgr;
			dialogMgr.onChoiceChanged = (Action)Delegate.Combine(dialogMgr.onChoiceChanged, new Action(OnChoiceChanged));
			_initialized = true;
		}
	}

	public void Close()
	{
		if (_initialized)
		{
			UINpcSpeechBox uINpcSpeechBox = npcSpeechBox;
			uINpcSpeechBox.onQuestItemClick = (Action<UINpcQuestItem>)Delegate.Remove(uINpcSpeechBox.onQuestItemClick, new Action<UINpcQuestItem>(OnQuestItemClick));
			UINpcSpeechBox uINpcSpeechBox2 = npcSpeechBox;
			uINpcSpeechBox2.onSetItemContent = (Action<UINpcQuestItem>)Delegate.Remove(uINpcSpeechBox2.onSetItemContent, new Action<UINpcQuestItem>(OnSetQuestItemContent));
			UINpcSpeechBox uINpcSpeechBox3 = npcSpeechBox;
			uINpcSpeechBox3.onUIClick = (Action)Delegate.Remove(uINpcSpeechBox3.onUIClick, new Action(OnUIClick));
			DialogMgr dialogMgr = PeCustomScene.Self.scenario.dialogMgr;
			dialogMgr.onChoiceChanged = (Action)Delegate.Remove(dialogMgr.onChoiceChanged, new Action(OnChoiceChanged));
			_initialized = false;
		}
	}

	private void OnChoiceChanged()
	{
		if (npcSpeechBox.IsChoice)
		{
			SetChoiceCount();
		}
	}

	private void OnSetQuestItemContent(UINpcQuestItem item)
	{
		if (_choices == null)
		{
			Debug.LogWarning("The giving dialog is null");
		}
		else if (item.index < 0 || item.index >= _choices.Count)
		{
			Debug.LogWarning("The index is out of range");
		}
		else
		{
			item.test = _choices[item.index];
		}
	}

	private void OnQuestItemClick(UINpcQuestItem item)
	{
		if (item.index < 0 || item.index >= _choices.Count)
		{
			Debug.LogWarning("The index is out of range");
			return;
		}
		int choiceId = PeCustomScene.Self.scenario.dialogMgr.GetChoiceId(item.index);
		if (choiceId != -1)
		{
			if (this.onChoiceClickForward != null)
			{
				this.onChoiceClickForward(choiceId);
			}
			if (this.onChoiceClick != null)
			{
				this.onChoiceClick(choiceId);
			}
			Debug.Log("Click the choice id [" + choiceId + "]");
		}
		else
		{
			Debug.LogWarning("cant find the quest id");
		}
	}

	private void OnUIClick()
	{
		if (this.onUIClick != null)
		{
			this.onUIClick();
		}
	}
}
