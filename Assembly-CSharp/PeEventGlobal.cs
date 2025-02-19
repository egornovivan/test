public class PeEventGlobal : Singleton<PeEventGlobal>
{
	public DeathEvent DeathEvent;

	public PickupEvent PickupEvent;

	public HPChangeEvent HPChangeEvent;

	public HPChangeEvent HPReduceEvent;

	public HPChangeEvent HPRecoverEvent;

	public ReviveEvent ReviveEvent;

	public DestroyEvent DestroyEvent;

	public MainPlayerAttackEvent MainPlayerAttack;

	public void Awake()
	{
		DeathEvent = new DeathEvent();
		PickupEvent = new PickupEvent();
		HPChangeEvent = new HPChangeEvent();
		HPReduceEvent = new HPChangeEvent();
		HPRecoverEvent = new HPChangeEvent();
		ReviveEvent = new ReviveEvent();
		DestroyEvent = new DestroyEvent();
		MainPlayerAttack = new MainPlayerAttackEvent();
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		DeathEvent.RemoveAllListeners();
		PickupEvent.RemoveAllListeners();
		HPChangeEvent.RemoveAllListeners();
		HPReduceEvent.RemoveAllListeners();
		HPRecoverEvent.RemoveAllListeners();
		ReviveEvent.RemoveAllListeners();
		DestroyEvent.RemoveAllListeners();
		MainPlayerAttack.RemoveAllListeners();
	}
}
