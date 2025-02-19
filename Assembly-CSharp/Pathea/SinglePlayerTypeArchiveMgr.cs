using System.IO;

namespace Pathea;

internal class SinglePlayerTypeArchiveMgr : ArchivableSingleton<SinglePlayerTypeArchiveMgr>
{
	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private SinglePlayerTypeLoader mSingleScenario;

	public SinglePlayerTypeLoader singleScenario
	{
		get
		{
			return mSingleScenario;
		}
		private set
		{
			mSingleScenario = value;
		}
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			mSingleScenario = new SinglePlayerTypeLoader();
			mSingleScenario.Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		mSingleScenario.Export(bw);
	}

	public override void New()
	{
		base.New();
		mSingleScenario = new SinglePlayerTypeLoader();
	}
}
