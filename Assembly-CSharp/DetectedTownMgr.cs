using System;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using PeMap;
using UnityEngine;

public class DetectedTownMgr : MonoLikeSingleton<DetectedTownMgr>
{
	public delegate void AddOrRemoveDetectedTownResult(IntVector2 PosCenter);

	public List<IntVector2> detectedTowns = new List<IntVector2>();

	public Dictionary<IntVector2, DetectedTown> DTownsDict = new Dictionary<IntVector2, DetectedTown>();

	public List<DetectedTown> AllTowns => DTownsDict.Values.ToList();

	public event AddOrRemoveDetectedTownResult AddDetectedTownListener;

	public event AddOrRemoveDetectedTownResult RemoveDetectedTownListener;

	public void RegistAtFirst()
	{
		PeSingleton<LabelMgr>.Instance.eventor.Subscribe(AddStoryCampByLabel);
	}

	public DetectedTown GetTown(IntVector2 posCenter)
	{
		if (DTownsDict.ContainsKey(posCenter))
		{
			return DTownsDict[posCenter];
		}
		return null;
	}

	public void RegisterAddDetectedTownListener(AddOrRemoveDetectedTownResult listener)
	{
		this.AddDetectedTownListener = (AddOrRemoveDetectedTownResult)Delegate.Combine(this.AddDetectedTownListener, listener);
	}

	public void UnregisterAddDetectedTownListener(AddOrRemoveDetectedTownResult listener)
	{
		this.AddDetectedTownListener = (AddOrRemoveDetectedTownResult)Delegate.Remove(this.AddDetectedTownListener, listener);
	}

	public void RegisterRemoveDetectedTownListener(AddOrRemoveDetectedTownResult listener)
	{
		this.RemoveDetectedTownListener = (AddOrRemoveDetectedTownResult)Delegate.Combine(this.RemoveDetectedTownListener, listener);
	}

	public void UnregisterRemoveDetectedTownListener(AddOrRemoveDetectedTownResult listener)
	{
		this.RemoveDetectedTownListener = (AddOrRemoveDetectedTownResult)Delegate.Remove(this.RemoveDetectedTownListener, listener);
	}

	public void AddDetectedTown(VArtifactTown vat)
	{
		if (!VArtifactTownManager.Instance.IsCaptured(vat.townId) && !detectedTowns.Contains(vat.PosCenter))
		{
			detectedTowns.Add(vat.PosCenter);
			DetectedTown detectedTown = new DetectedTown(vat);
			DTownsDict.Add(detectedTown.PosCenter, detectedTown);
			if (this.AddDetectedTownListener != null)
			{
				this.AddDetectedTownListener(detectedTown.PosCenter);
			}
		}
	}

	public void RemoveDetectedTown(VArtifactTown vat)
	{
		if (detectedTowns.Contains(vat.PosCenter))
		{
			detectedTowns.Remove(vat.PosCenter);
			DTownsDict.Remove(vat.PosCenter);
			if (this.RemoveDetectedTownListener != null)
			{
				this.RemoveDetectedTownListener(vat.PosCenter);
			}
		}
	}

	public void AddStoryCampByLabel(object sender, LabelMgr.Args arg)
	{
		if (!arg.add || !(arg.label is StaticPoint { campId: >0, campId: var campId } staticPoint) || !CampTradeIdData.IsStoryDetectTradeCamp(campId))
		{
			return;
		}
		Vector3 position = staticPoint.position;
		Camp camp = Camp.GetCamp(campId);
		if (camp != null)
		{
			DetectedTown detectedTown = new DetectedTown(position, camp.Name, campId);
			DTownsDict.Add(detectedTown.PosCenter, detectedTown);
			if (this.AddDetectedTownListener != null)
			{
				this.AddDetectedTownListener(detectedTown.PosCenter);
			}
		}
	}

	public void AddStoryCampByMission(int campId)
	{
		if (campId <= 0 || !CampTradeIdData.IsStoryMissionTradeCamp(campId))
		{
			return;
		}
		Camp camp = Camp.GetCamp(campId);
		if (camp != null)
		{
			DetectedTown detectedTown = new DetectedTown(camp.Pos, camp.Name, campId);
			DTownsDict.Add(detectedTown.PosCenter, detectedTown);
			if (this.AddDetectedTownListener != null)
			{
				this.AddDetectedTownListener(detectedTown.PosCenter);
			}
		}
	}
}
