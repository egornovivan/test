namespace Pathea;

public interface IPeMsg
{
	void OnMsg(EMsg msg, params object[] args);
}
