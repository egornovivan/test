using System;

namespace SkillSystem;

public interface ICompilableExp
{
	void OnCompiled(Action<ISkAttribs, ISkAttribs, ISkAttribsModPara> action);
}
