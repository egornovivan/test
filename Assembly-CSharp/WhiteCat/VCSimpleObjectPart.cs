using Pathea.Operate;

namespace WhiteCat;

public abstract class VCSimpleObjectPart : VCPart
{
	private bool init;

	private PEBed _pePed;

	private CmdList cmdList = new CmdList();

	private PEBed PeBed
	{
		get
		{
			if (!init)
			{
				init = true;
				_pePed = GetComponentInParent<PEBed>();
			}
			return _pePed;
		}
	}

	public bool CanRecycle()
	{
		return PeBed == null || PeBed.IsIdle();
	}

	public virtual CmdList GetCmdList()
	{
		cmdList.Clear();
		return cmdList;
	}

	public bool CanRotateObject()
	{
		return PeBed == null || PeBed.IsIdle();
	}
}
