using System.IO;

namespace Pathea;

internal class MultiPlayerTypeArchiveMgr : ArchivableSingleton<MultiPlayerTypeArchiveMgr>
{
	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private MultiPlayerTypeLoader mMultiScenario;

	public MultiPlayerTypeLoader multiScenario
	{
		get
		{
			return mMultiScenario;
		}
		private set
		{
			mMultiScenario = value;
		}
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override void SetData(byte[] data)
	{
	}

	protected override void WriteData(BinaryWriter bw)
	{
		mMultiScenario.Export(bw);
	}

	public override void New()
	{
		base.New();
		mMultiScenario = new MultiPlayerTypeLoader();
	}
}
