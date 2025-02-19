using System.Collections.Generic;
using System.Linq;
using Pathea;
using PeMap;
using UnityEngine;

public class RandomMapIconMgr
{
	public static Dictionary<Vector3, TownLabel> npcTownLabel = new Dictionary<Vector3, TownLabel>();

	public static Dictionary<Vector3, TownLabel> nativeCampLabel = new Dictionary<Vector3, TownLabel>();

	public static Dictionary<Vector3, TownLabel> destroyedTownLabel = new Dictionary<Vector3, TownLabel>();

	public static bool HasTownLabel(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is TownLabel townLabel && townLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return true;
		}
		return false;
	}

	public static void AddTownIcon(VArtifactTown vat)
	{
		if (vat != null && !vat.isEmpty && !NpcTownLabel.ContainsIcon(vat.TransPos))
		{
			UnknownLabel.Remove(vat.TransPos);
			NpcTownLabel npcTownLabel = new NpcTownLabel(vat);
			if (!RandomMapIconMgr.npcTownLabel.ContainsKey(npcTownLabel.pos))
			{
				RandomMapIconMgr.npcTownLabel.Add(npcTownLabel.pos, npcTownLabel);
			}
		}
	}

	public static void AddNativeIcon(VArtifactTown vat)
	{
		if (vat != null && !vat.isEmpty && !NativeLabel.ContainsIcon(vat.TransPos))
		{
			UnknownLabel.Remove(vat.TransPos);
			NativeLabel nativeLabel = new NativeLabel(vat);
			if (!nativeCampLabel.ContainsKey(nativeLabel.pos))
			{
				nativeCampLabel.Add(nativeLabel.pos, nativeLabel);
			}
		}
	}

	public static void DestroyTownIcon(VArtifactTown vat)
	{
		if (vat == null)
		{
			return;
		}
		if (vat.type == VArtifactType.NpcTown)
		{
			npcTownLabel.Remove(vat.TransPos);
		}
		else
		{
			nativeCampLabel.Remove(vat.TransPos);
		}
		TownLabel.Remove(vat.TransPos);
		if (!DestroyedLabel.ContainsIcon(vat.TransPos))
		{
			UnknownLabel.Remove(vat.TransPos);
			DestroyedLabel destroyedLabel = new DestroyedLabel(vat);
			if (!destroyedTownLabel.ContainsKey(destroyedLabel.pos))
			{
				destroyedTownLabel.Add(destroyedLabel.pos, destroyedLabel);
			}
		}
	}

	public static void AddDestroyedTownIcon(VArtifactTown vat)
	{
		if (vat != null && !vat.isEmpty && !DestroyedLabel.ContainsIcon(vat.TransPos))
		{
			UnknownLabel.Remove(vat.TransPos);
			DestroyedLabel destroyedLabel = new DestroyedLabel(vat);
			if (!destroyedTownLabel.ContainsKey(destroyedLabel.pos))
			{
				destroyedTownLabel.Add(destroyedLabel.pos, destroyedLabel);
			}
		}
	}

	public static void ClearAll()
	{
		TownLabel[] tls = npcTownLabel.Values.ToArray();
		TownLabel[] nls = nativeCampLabel.Values.ToArray();
		TownLabel[] dls = destroyedTownLabel.Values.ToArray();
		PeSingleton<LabelMgr>.Instance.RemoveAll((ILabel it) => tls.Contains(it) || nls.Contains(it) || dls.Contains(it));
		npcTownLabel = new Dictionary<Vector3, TownLabel>();
		nativeCampLabel = new Dictionary<Vector3, TownLabel>();
		destroyedTownLabel = new Dictionary<Vector3, TownLabel>();
	}
}
