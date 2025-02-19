using System.Collections.Generic;

namespace Pathea;

public interface Team
{
	List<PeEntity> GetTeamMembers();

	bool AddInTeam(List<PeEntity> members, bool Isclear = true);

	bool DissolveTeam();
}
