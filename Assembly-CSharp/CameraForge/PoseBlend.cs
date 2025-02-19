using System;
using System.IO;
using UnityEngine;

namespace CameraForge;

public class PoseBlend : PoseNode
{
	public Slot Name;

	public Slot Count;

	public Slot Index;

	public PoseSlot[] Poses;

	private float[] Weights;

	private float CrossFadeSpeed = 1f;

	public int currentIndex => Mathf.RoundToInt(Index.value.value_f) % Poses.Length;

	public override PoseSlot[] poseslots
	{
		get
		{
			PoseSlot[] array = new PoseSlot[Poses.Length];
			Array.Copy(Poses, array, Poses.Length);
			return array;
		}
	}

	public override Slot[] slots => new Slot[3] { Name, Count, Index };

	public PoseBlend()
	{
		Name = new Slot("Name");
		Count = new Slot("Count");
		Index = new Slot("Index");
		Poses = new PoseSlot[0];
		Name.value = "Pose Blend";
		UpdateCount(2);
		Index.value = 0;
	}

	public void UpdateCount(int count)
	{
		count = Mathf.Clamp(count, 1, 16);
		Count.input = null;
		Count.value = count;
		PoseSlot[] array = new PoseSlot[count];
		int b = Poses.Length;
		int length = Mathf.Min(count, b);
		Array.Copy(Poses, array, length);
		for (int i = 0; i < count; i++)
		{
			if (array[i] == null)
			{
				array[i] = new PoseSlot("P[" + i + "]");
			}
		}
		Poses = array;
		Weights = new float[count];
		Weights[currentIndex] = 1f;
	}

	public override Pose Calculate()
	{
		Fade();
		Pose zero = Pose.Zero;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		float num4 = 0f;
		float num5 = 0f;
		for (int i = 0; i < Poses.Length; i++)
		{
			if (!(Weights[i] < 1E-06f))
			{
				Poses[i].Calculate();
				num += (double)(Poses[i].value.position.x * Weights[i]);
				num2 += (double)(Poses[i].value.position.y * Weights[i]);
				num3 += (double)(Poses[i].value.position.z * Weights[i]);
				zero.rotation = Quaternion.Slerp(Poses[i].value.rotation, zero.rotation, num4 / (num4 + Weights[i]));
				zero.fov += Poses[i].value.fov * Weights[i];
				zero.nearClip += Poses[i].value.nearClip * Weights[i];
				num5 += ((!Poses[i].value.lockCursor) ? 0f : 1f) * Weights[i];
				zero.cursorPos += Poses[i].value.cursorPos * Weights[i];
				zero.saturate += Poses[i].value.saturate * Weights[i];
				zero.motionBlur += Poses[i].value.motionBlur * Weights[i];
				num4 += Weights[i];
			}
		}
		num /= (double)num4;
		num2 /= (double)num4;
		num3 /= (double)num4;
		zero.position = new Vector3((float)num, (float)num2, (float)num3);
		zero.fov /= num4;
		zero.nearClip /= num4;
		num5 /= num4;
		zero.lockCursor = num5 > 0.99f;
		zero.cursorPos /= num4;
		zero.saturate /= num4;
		zero.motionBlur /= num4;
		return zero;
	}

	internal override void Read(BinaryReader r)
	{
		if (controller == null)
		{
			Debug.LogError("no modifier!");
			return;
		}
		editorPos.x = r.ReadSingle();
		editorPos.y = r.ReadSingle();
		Slot[] array = slots;
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string text = r.ReadString();
			Slot slot = null;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].name == text)
				{
					slot = array[j];
					break;
				}
			}
			if (slot != null)
			{
				int num2 = r.ReadInt32();
				if (num2 != -1)
				{
					slot.input = null;
					if (num2 >= 0 && num2 < controller.nodes.Count)
					{
						slot.input = controller.nodes[num2];
					}
				}
				else
				{
					slot.input = null;
					slot.value.Read(r);
				}
			}
			else
			{
				int num3 = r.ReadInt32();
				if (num3 == -1)
				{
					Var @null = Var.Null;
					@null.Read(r);
				}
			}
		}
		UpdateCount(Mathf.RoundToInt(Count.value.value_f));
		PoseSlot[] array2 = poseslots;
		int num4 = r.ReadInt32();
		for (int k = 0; k < num4; k++)
		{
			string text2 = r.ReadString();
			PoseSlot poseSlot = null;
			for (int l = 0; l < array2.Length; l++)
			{
				if (array2[l].name == text2)
				{
					poseSlot = array2[l];
					break;
				}
			}
			int num5 = r.ReadInt32();
			if (poseSlot == null)
			{
				continue;
			}
			if (num5 != -1)
			{
				poseSlot.input = null;
				if (num5 >= 0 && num5 < controller.posenodes.Count)
				{
					poseSlot.input = controller.posenodes[num5];
				}
				if (poseSlot.input == this)
				{
					poseSlot.input = null;
				}
			}
			else
			{
				poseSlot.input = null;
				poseSlot.value = Pose.Default;
			}
		}
		int count = r.ReadInt32();
		data = r.ReadBytes(count);
	}

	public float GetWeight(int index)
	{
		return Weights[index];
	}

	private void Fade()
	{
		for (int i = 0; i < Weights.Length; i++)
		{
			if (i == currentIndex)
			{
				Weights[i] = Mathf.Lerp(Weights[i], 1f, CrossFadeSpeed);
				if (Weights[i] > 0.999999f)
				{
					Weights[i] = 1f;
				}
			}
			else
			{
				Weights[i] = Mathf.Lerp(Weights[i], 0f, CrossFadeSpeed);
				if (Weights[i] < 1E-06f)
				{
					Weights[i] = 0f;
				}
			}
		}
	}

	public void CrossFade(int index, float speed = 0.3f)
	{
		Index.input = null;
		Index.value = index;
		CrossFadeSpeed = speed;
		if (speed < 0.001f)
		{
			speed = 0.001f;
		}
		if (speed > 1f)
		{
			speed = 1f;
		}
	}
}
