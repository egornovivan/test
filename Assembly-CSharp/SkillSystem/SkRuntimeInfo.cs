namespace SkillSystem;

public abstract class SkRuntimeInfo
{
	public static SkRuntimeInfo Current;

	public abstract ISkPara Para { get; }

	public abstract SkEntity Caster { get; }

	public abstract SkEntity Target { get; }
}
