using System.Collections.Generic;
using Pathea;

public class Scanner : PeCmpt
{
	private const int VersionID = 1;

	private MSScan _msScaner;

	private int _radius = 80;

	private int _radiusaddition;

	private List<byte> _matList = new List<byte>();

	private List<byte> _additionMatList = new List<byte>();

	public int Radius
	{
		get
		{
			return _radius + _radiusaddition;
		}
		set
		{
			_radiusaddition = value;
		}
	}

	public List<byte> GetMatList()
	{
		return _additionMatList;
	}

	public void Add(byte mat)
	{
		if (!_additionMatList.Contains(mat))
		{
			_additionMatList.Add(mat);
		}
	}

	public void ResetMat()
	{
		_additionMatList.Clear();
		_additionMatList.AddRange(_matList);
	}

	public override void Start()
	{
		base.Start();
		_msScaner = base.gameObject.AddComponent<MSScan>();
	}

	public void Clear()
	{
		_matList.Clear();
		_radius = 0;
	}

	public void AddMat(byte mat)
	{
		if (_matList != null && !_matList.Contains(mat))
		{
			_matList.Add(mat);
		}
	}

	public void RemoveMat(byte mat)
	{
		_matList.Remove(mat);
	}

	public void Scan()
	{
		_msScaner.MakeAScan(base.transform.position, _matList, Radius);
	}
}
