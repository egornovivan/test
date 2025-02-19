public class TmpSurfExtractor : IVxSurfExtractor
{
	private static TmpSurfExtractor _inst;

	public static TmpSurfExtractor Inst
	{
		get
		{
			if (_inst == null)
			{
				_inst = new TmpSurfExtractor();
			}
			return _inst;
		}
	}

	public bool IsIdle => true;

	public bool IsAllClear => true;

	public bool Pause { get; set; }

	private TmpSurfExtractor()
	{
	}

	public void Init()
	{
	}

	public void Exec()
	{
	}

	public int OnFin()
	{
		return 0;
	}

	public void Reset()
	{
	}

	public void CleanUp()
	{
	}

	public bool AddSurfExtractReq(IVxSurfExtractReq req)
	{
		return true;
	}
}
