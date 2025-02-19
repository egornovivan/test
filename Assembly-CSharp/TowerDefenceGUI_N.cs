public class TowerDefenceGUI_N : UIBaseWnd
{
	public UILabel mLeftWave;

	public UILabel mWaveTime;

	public UISlider mLifeBar;

	private int mWaveTimeCount;

	protected override void InitWindow()
	{
		base.InitWindow();
		Hide();
	}

	public void SetCenterLife(float lifePresent)
	{
		mLifeBar.sliderValue = lifePresent;
	}

	public void SetState(int leftWave, int waveTime)
	{
		mLeftWave.text = leftWave.ToString();
		if (mWaveTimeCount != waveTime)
		{
			mWaveTimeCount = waveTime;
			string text = string.Empty;
			int num = waveTime / 60;
			int num2 = waveTime % 60;
			if (num < 10)
			{
				text += "0";
			}
			text += num;
			text += ":";
			if (num2 < 10)
			{
				text += "0";
			}
			text += num2;
			mWaveTime.text = text;
		}
	}
}
