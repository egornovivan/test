using System.Collections.Generic;

namespace Pathea;

public class NpcThinking
{
	public int ID;

	public string Name;

	public EThinkingType Type;

	public Dictionary<EThinkingType, EThinkingMask> mThinkInfo;

	public NpcThinking()
	{
		mThinkInfo = new Dictionary<EThinkingType, EThinkingMask>();
	}

	public EThinkingMask GetMask(EThinkingType type)
	{
		return mThinkInfo[type];
	}
}
