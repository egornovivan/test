using System.Collections.Generic;

namespace ItemAsset;

public class RecycleCreation : Recycle
{
	public override List<MaterialItem> GetResItemList()
	{
		if (itemObj.protoId < 100000000)
		{
			return null;
		}
		return base.protoData.repairMaterialList;
	}
}
