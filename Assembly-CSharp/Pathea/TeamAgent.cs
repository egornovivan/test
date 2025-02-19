using System.Collections.Generic;

namespace Pathea;

public abstract class TeamAgent : Team
{
	internal List<PeEntity> teamMembers;

	public TeamAgent()
	{
	}

	public virtual void InitTeam()
	{
		teamMembers = new List<PeEntity>();
	}

	public abstract List<PeEntity> GetTeamMembers();

	public abstract bool ReFlashTeam();

	public abstract bool ClearTeam();

	public abstract bool RemoveFromTeam(PeEntity members);

	public abstract bool ContainMember(PeEntity members);

	public abstract bool RemoveFromTeam(List<PeEntity> members);

	public abstract bool AddInTeam(List<PeEntity> members, bool Isclear = true);

	public abstract bool AddInTeam(PeEntity members);

	public abstract void OnAlertInform(PeEntity enemy);

	public abstract void OnClearAlert();

	public abstract bool DissolveTeam();
}
