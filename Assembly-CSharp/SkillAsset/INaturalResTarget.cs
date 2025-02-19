using System.Collections.Generic;
using ItemAsset;

namespace SkillAsset;

public interface INaturalResTarget : ISkillTarget
{
	int GetDestroyed(SkillRunner caster, float durDec, float radius);

	List<ItemSample> ReturnItems(short resGotMultiplier, int num);

	bool IsDestroyed();
}
