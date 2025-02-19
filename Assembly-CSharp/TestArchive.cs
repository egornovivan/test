using System.IO;
using Pathea;
using UnityEngine;

public class TestArchive : MonoBehaviour
{
	public class ArchiveTestData : ArchivableSingleton<ArchiveTestData>
	{
		protected override string GetRecordName()
		{
			return "testArchive";
		}

		protected override bool GetSaveFlagResetValue()
		{
			return false;
		}

		protected override void WriteData(BinaryWriter bw)
		{
			Debug.Log("save test archive");
		}

		protected override void SetData(byte[] data)
		{
			Debug.Log("load test archive");
		}
	}

	[SerializeField]
	private bool saveMe;

	private void Update()
	{
		if (saveMe)
		{
			saveMe = false;
			PeSingleton<ArchiveTestData>.Instance.SaveMe();
		}
	}
}
