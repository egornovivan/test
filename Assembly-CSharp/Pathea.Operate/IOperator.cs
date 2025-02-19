namespace Pathea.Operate;

public interface IOperator
{
	IOperation Operate { get; set; }

	bool IsActionRunning(PEActionType type);

	bool DoAction(PEActionType type, PEActionParam para = null);

	void EndAction(PEActionType type);
}
