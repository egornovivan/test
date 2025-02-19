using System.Collections.Generic;
using UnityEngine;

public class TrajectoryVisualizer
{
	private Color color;

	private float length;

	private bool dotted;

	private List<TimePoint> trajectory = new List<TimePoint>();

	public TrajectoryVisualizer(Color color, float length)
	{
		this.color = color;
		this.length = length;
	}

	public void AddPoint(float time, Vector3 point)
	{
		trajectory.Add(new TimePoint(time, point));
		while (trajectory[0].time < time - length)
		{
			trajectory.RemoveAt(0);
		}
	}

	public void Render()
	{
		if (trajectory.Count != 0)
		{
			DrawArea drawArea = new DrawArea3D(Vector3.zero, Vector3.one, Matrix4x4.identity);
			float time = trajectory[trajectory.Count - 1].time;
			GL.Begin(1);
			for (int i = 0; i < trajectory.Count - 1; i++)
			{
				Color c = color;
				c.a = (time - trajectory[i].time) / length;
				c.a = 1f - c.a * c.a;
				drawArea.DrawLine(trajectory[i].point, trajectory[i + 1].point, c);
			}
			GL.End();
		}
	}
}
