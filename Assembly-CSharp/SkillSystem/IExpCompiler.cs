namespace SkillSystem;

public interface IExpCompiler
{
	void Compile();

	void AddExpString(ICompilableExp op, string strExp);
}
