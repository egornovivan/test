using uLink;

public class AiMissionMonster : AiMonsterNetwork
{
	protected int _ownerId = -1;

	public int OwnerId => _ownerId;

	protected override void OnPEInstantiate(NetworkMessageInfo info)
	{
		base.OnPEInstantiate(info);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
	}
}
