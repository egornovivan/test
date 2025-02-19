using UnityEngine;

public class LagTester : MonoBehaviour
{
	public GUISkin gskin;

	private HighStopwatch watch;

	private long counter;

	public int frame;

	public double time;

	public static double threshold = 10000.0;

	private string text = string.Empty;

	private int showtextCounter;

	private void Start()
	{
		try
		{
			watch = new HighStopwatch();
		}
		catch
		{
		}
		if (watch != null)
		{
			counter = watch.Value;
		}
	}

	private void Update()
	{
		if (!(threshold > 1000.0))
		{
			double num = UpdateTime();
			if (num > threshold)
			{
				text = "Lag detected : " + (num * 1000.0).ToString("0.0") + " ms";
				showtextCounter = 200;
			}
		}
	}

	private double UpdateTime()
	{
		if (watch == null || watch.Frequency == 0L)
		{
			time = 0.0;
			return 0.0;
		}
		long num = counter;
		counter = watch.Value;
		double num2 = (double)(counter - num) / (double)watch.Frequency;
		time += num2;
		frame++;
		return num2;
	}

	private void OnGUI()
	{
		showtextCounter--;
		if (showtextCounter > 0)
		{
			GUI.skin = gskin;
			GUI.color = Color.yellow;
			GUI.Label(new Rect(Screen.width - 152, Screen.height - 22, 180f, 20f), text);
			GUI.color = Color.white;
		}
	}
}
