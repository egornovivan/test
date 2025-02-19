using System.IO;
using Pathea;
using UnityEngine;

public class TestSwapSpace : MonoBehaviour
{
	[SerializeField]
	private string dir;

	[SerializeField]
	private string copyInDir;

	[SerializeField]
	private string copyOutDir;

	[SerializeField]
	private bool sync;

	private SwapSpace mSwapSpace;

	private void Start()
	{
		if (string.IsNullOrEmpty(dir))
		{
			dir = Path.Combine(Directory.GetCurrentDirectory(), "test_swap");
		}
		if (string.IsNullOrEmpty(copyInDir))
		{
			copyInDir = Path.Combine(Directory.GetCurrentDirectory(), "archive");
		}
		if (string.IsNullOrEmpty(copyOutDir))
		{
			copyOutDir = Path.Combine(Directory.GetCurrentDirectory(), "archive1");
		}
		mSwapSpace = new SwapSpace();
		mSwapSpace.Init(dir);
	}

	private void Update()
	{
		if (sync)
		{
			sync = false;
			mSwapSpace.CopyFrom(copyInDir);
			mSwapSpace.CopyTo(copyOutDir);
		}
	}
}
