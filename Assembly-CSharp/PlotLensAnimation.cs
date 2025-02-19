using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class PlotLensAnimation
{
	private static int plotClipNum;

	private static bool isPlaying;

	private static List<CameraInfo> plotClips;

	private static Dictionary<int, CutsceneClip> talkId_cut;

	public static bool IsPlaying
	{
		get
		{
			return isPlaying;
		}
		set
		{
			isPlaying = value;
		}
	}

	private static Dictionary<int, CutsceneClip> TalkId_cut
	{
		get
		{
			if (talkId_cut == null)
			{
				talkId_cut = new Dictionary<int, CutsceneClip>();
			}
			return talkId_cut;
		}
		set
		{
			talkId_cut = value;
		}
	}

	public static void PlotPlay(List<CameraInfo> clipsID)
	{
		if (!isPlaying)
		{
			plotClips = clipsID;
			plotClipNum = 0;
			CutsceneClip cutsceneClip = Cutscene.PlayClip(plotClips[plotClipNum].cameraId);
			TalkId_cut[plotClips[plotClipNum].talkId] = cutsceneClip;
			cutsceneClip.onArriveAtEnding.AddListener(PlotNextPlay);
			isPlaying = true;
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr.DoAction(PEActionType.Cutscene);
				PESkEntity peSkEntity = PeSingleton<PeCreature>.Instance.mainPlayer.peSkEntity;
				peSkEntity.SetAttribute(AttribType.CampID, 28f);
				peSkEntity.SetAttribute(AttribType.DamageID, 28f);
			}
		}
		else
		{
			plotClips.AddRange(clipsID);
		}
	}

	public static bool TooFar(List<int> clipsID)
	{
		foreach (int item in clipsID)
		{
			if (Cutscene.TooFar(item))
			{
				return true;
			}
		}
		return false;
	}

	private static void PlotNextPlay()
	{
		plotClipNum++;
		if (plotClipNum < plotClips.Count)
		{
			CutsceneClip cutsceneClip = Cutscene.PlayClip(plotClips[plotClipNum].cameraId);
			TalkId_cut[plotClips[plotClipNum].talkId] = cutsceneClip;
			cutsceneClip.onArriveAtEnding.AddListener(PlotNextPlay);
			return;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr.EndAction(PEActionType.Cutscene);
			PESkEntity peSkEntity = PeSingleton<PeCreature>.Instance.mainPlayer.peSkEntity;
			peSkEntity.SetAttribute(AttribType.CampID, 1f);
			peSkEntity.SetAttribute(AttribType.DamageID, 1f);
		}
		plotClipNum = 0;
		TalkId_cut.Clear();
		isPlaying = false;
	}

	public static void CheckIsStopCamera(int talkId)
	{
		if (talkId != 0 && TalkId_cut.ContainsKey(talkId))
		{
			Object.Destroy(TalkId_cut[talkId].gameObject);
			TalkId_cut.Remove(talkId);
		}
	}
}
