using System;
using System.IO;
using ItemAsset;
using UnityEngine;

namespace Pathea;

public class ItemAssetArchiveMgr : ArchivableSingleton<ItemAssetArchiveMgr>
{
	private const string ArchiveKey = "ArchiveKeyItemAsset";

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyItemAsset";
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			try
			{
				PeSingleton<ItemMgr>.Instance.Import(data);
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
			}
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		PeSingleton<ItemMgr>.Instance.Export(bw);
	}
}
