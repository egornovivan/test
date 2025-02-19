using Pathea;

namespace Behave.Runtime.Action;

public interface IAttack
{
	float Weight { get; }

	bool CanInterrupt();

	bool IsRunning(Enemy enemy);

	bool IsReadyCD(Enemy enemy);

	bool ReadyAttack(Enemy enemy);

	bool CanAttack(Enemy enemy);

	bool IsBlocked(Enemy enemy);
}
