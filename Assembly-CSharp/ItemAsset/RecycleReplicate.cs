using System.Collections.Generic;
using Pathea;

namespace ItemAsset;

public class RecycleReplicate : Recycle
{
	public override List<MaterialItem> GetResItemList()
	{
		if (itemObj.protoId >= 100000000)
		{
			return null;
		}
		Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.FindByProductId(itemObj.protoId);
		if (formula == null)
		{
			return null;
		}
		List<MaterialItem> list = new List<MaterialItem>(10);
		foreach (Replicator.Formula.Material material in formula.materials)
		{
			list.Add(new MaterialItem
			{
				protoId = material.itemId,
				count = material.itemCount
			});
		}
		return list;
	}
}
