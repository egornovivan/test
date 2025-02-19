using uLink;

public class NativeTowerGiftNetwork : AiMonsterNetwork
{
	protected override void OnPEInstantiate(NetworkMessageInfo info)
	{
		base.OnPEInstantiate(info);
		Invoke("SelfKill", 1f);
	}

	public void SelfKill()
	{
	}
}
