using Pathea;

public class PEAE_HumanAudio : PEAbnormalEff
{
	public int audioID { get; set; }

	public int sex { get; set; }

	public PeEntity entity { get; set; }

	public override void Do()
	{
		if (!(null == entity) && (sex == 1 || sex == 2))
		{
			int[] soundID = HumanSoundData.GetSoundID(audioID, sex);
			for (int i = 0; i < soundID.Length; i++)
			{
				AudioManager.instance.Create(entity.position, soundID[i]);
			}
		}
	}
}
