using System;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;

public class NpcRandomTalkAudio
{
	private static Dictionary<int, AudioController> contrillers = new Dictionary<int, AudioController>();

	private static List<int> secnarioIDs = new List<int>();

	public static bool PlaySound(PeEntity npc, int caseId, int secnarioID)
	{
		if (AudioManager.instance == null)
		{
			return false;
		}
		if (secnarioIDs.Contains(caseId))
		{
			return false;
		}
		if (npc.NpcCmpt != null && npc.NpcCmpt.voiceType <= 0)
		{
			RandomNpcDb.Item item = RandomNpcDb.Get(npc.ProtoID);
			if (item != null && npc.commonCmpt != null)
			{
				npc.ExtSetVoiceType(item.voiveMatch.GetRandomVoice(npc.commonCmpt.sex));
			}
		}
		if (npc.NpcCmpt != null && npc.NpcCmpt.voiceType <= 0)
		{
			return false;
		}
		int voiceId = NpcVoiceDb.GetVoiceId(secnarioID, npc.NpcCmpt.voiceType);
		if (voiceId > 0 && null != npc.peTrans)
		{
			AudioController audioController = AudioManager.instance.Create(npc.position, voiceId, npc.peTrans.realTrans);
			audioController.DestroyEvent = (Action<AudioController>)Delegate.Combine(audioController.DestroyEvent, new Action<AudioController>(OnDelete));
			contrillers.Add(caseId, audioController);
			secnarioIDs.Add(caseId);
			return true;
		}
		return false;
	}

	private static void OnDelete(AudioController audioCtrl)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < secnarioIDs.Count; i++)
		{
			if ((contrillers[secnarioIDs[i]] != null && contrillers[secnarioIDs[i]].Equals(audioCtrl)) || (contrillers[secnarioIDs[i]] != null && contrillers[secnarioIDs[i]].time >= contrillers[secnarioIDs[i]].length))
			{
				contrillers.Remove(secnarioIDs[i]);
				list.Add(secnarioIDs[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			secnarioIDs.Remove(list[j]);
		}
		audioCtrl.DestroyEvent = (Action<AudioController>)Delegate.Remove(audioCtrl.DestroyEvent, new Action<AudioController>(OnDelete));
	}
}
