using System.Collections.Generic;

namespace Pathea;

public class CmptMgr : MonoLikeSingleton<CmptMgr>
{
	private List<PeCmpt> _cmpts = new List<PeCmpt>();

	public override void Update()
	{
		int count = _cmpts.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			PeCmpt peCmpt = _cmpts[num];
			if (peCmpt != null && !peCmpt.Equals(null))
			{
				if (peCmpt.isActiveAndEnabled)
				{
					peCmpt.OnUpdate();
				}
			}
			else
			{
				_cmpts.RemoveAt(num);
			}
		}
	}

	public void AddCmpt(PeCmpt cmpt)
	{
		_cmpts.Add(cmpt);
	}

	public void RemoveCmpt(PeCmpt cmpt)
	{
		_cmpts.Remove(cmpt);
	}
}
