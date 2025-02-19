public interface IExpFuncSet
{
	bool RandIn(float prob);

	void GetHurt(float dmg);

	void TryGetHurt(float dmg, float exp = 0f);

	bool InCoolingForGettingHurt(float coolTime);

	bool InCoolingForConsumingStamina(float cooltime);
}
