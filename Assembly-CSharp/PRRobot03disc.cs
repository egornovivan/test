public class PRRobot03disc : PRGhost
{
	public WeaponTrail[] trails = new WeaponTrail[6];

	public new void Start()
	{
		base.Start();
		for (int i = 0; i < 6; i++)
		{
			trails[i].ClearTrail();
			trails[i].StartTrail(0.2f, 0.2f);
		}
	}
}
