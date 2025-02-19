using System.Collections.Generic;
using Pathea;

namespace SkillSystem;

internal class SkInstPool : MonoLikeSingleton<SkInstPool>
{
	public List<SkInst> _skInsts = new List<SkInst>();
}
