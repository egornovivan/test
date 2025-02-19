using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AbnormalCmpt : DataCmpt
{
	protected Dictionary<int, byte[]> _abnormalCond = new Dictionary<int, byte[]>();

	public AbnormalCmpt()
	{
		mType = ECmptType.Abnormal;
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, _abnormalCond.Count);
		for (int i = 0; i < _abnormalCond.Count; i++)
		{
			int key = _abnormalCond.ElementAt(i).Key;
			BufferHelper.Serialize(w, key);
			BufferHelper.Serialize(w, _abnormalCond[key]);
		}
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			int key = BufferHelper.ReadInt32(r);
			byte[] value = BufferHelper.ReadBytes(r);
			_abnormalCond[key] = value;
		}
	}

	public void ApplyAbnormalCondition(int type, byte[] data)
	{
		_abnormalCond[type] = data;
	}

	public void EndAbnormalCondition(int type)
	{
		_abnormalCond.Remove(type);
	}

	public IEnumerable<int> TreatedAbnormal()
	{
		foreach (KeyValuePair<int, byte[]> iter in _abnormalCond)
		{
			if (AbnormalTypeTreatData.CanBeTreatInColony(iter.Key))
			{
				yield return iter.Key;
			}
		}
	}
}
