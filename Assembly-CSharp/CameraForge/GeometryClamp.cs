using System;
using UnityEngine;

namespace CameraForge;

public class GeometryClamp : ScriptModifier
{
	private float rise;

	public override Slot[] slots => new Slot[2] { Name, Col };

	public override PoseSlot[] poseslots => new PoseSlot[1] { Prev };

	public override Pose Calculate()
	{
		Col.Calculate();
		Prev.Calculate();
		if (controller != null && controller.executor != null)
		{
			bool @bool = GetBool("Shoot Mode");
			float @float = GetFloat("Dist");
			LayerMask layerMask = controller.executor.GetVar("Obstacle LayerMask").value_i;
			float num = Mathf.Clamp(GetFloat("Activity Space Size") * 0.2f, 0.4f, 1f);
			Pose value = Prev.value;
			Vector3 vector = value.rotation * Vector3.forward;
			Vector3 vector2 = value.rotation * Vector3.right;
			Vector3 vector3 = value.rotation * Vector3.up;
			Vector3 vector4 = value.position + @float * vector;
			float aspect = controller.executor.camera.aspect;
			float num2 = Mathf.Sqrt(1f + aspect * aspect) * Mathf.Tan(value.fov * 0.5f * ((float)Math.PI / 180f));
			value.nearClip *= num;
			float num3 = Utils.EvaluateNearclipPlaneRadius(vector4, 0.05f, value.nearClip * num2, layerMask);
			controller.executor.SetFloat("NCR", num3);
			value.nearClip = num3 / num2;
			Vector3 vector5 = value.position + value.nearClip * vector;
			bool flag = false;
			for (float num4 = 0f; num4 < 3.01f; num4 += 1.5f)
			{
				int num5 = 8;
				if (num4 < 2f)
				{
					num5 = 4;
				}
				else if (num4 < 1f)
				{
					num5 = 1;
				}
				int num6 = 0;
				while (num6 < num5)
				{
					float f = (float)(num6 * (360 / num5)) * ((float)Math.PI / 180f);
					Vector3 vector6 = num4 * (Mathf.Cos(f) * vector2 + Mathf.Sin(f) * vector3);
					Vector3 start = vector6 + vector4;
					Ray ray = new Ray(vector4, vector6.normalized);
					if ((num4 != 0f && Physics.Raycast(ray, num4, layerMask, QueryTriggerInteraction.Ignore)) || Physics.Linecast(start, vector5, layerMask, QueryTriggerInteraction.Ignore))
					{
						num6++;
						continue;
					}
					goto IL_0261;
				}
				continue;
				IL_0261:
				flag = true;
				break;
			}
			if (!flag || Physics.OverlapSphere(vector5, num3, layerMask, QueryTriggerInteraction.Ignore).Length != 0)
			{
				float num7 = Vector3.Distance(vector4, vector5);
				float num8 = 0f;
				Vector3 normalized = (vector5 - vector4).normalized;
				float num9 = num7 - num8;
				if (!(num9 <= 0f))
				{
					Ray ray2 = new Ray(vector4 + normalized * num8, normalized);
					if (Physics.SphereCast(ray2, num3 - 0.01f, out var hitInfo, num9, layerMask, QueryTriggerInteraction.Ignore))
					{
						hitInfo.distance += num8;
						vector5 = vector4 + normalized * hitInfo.distance;
						controller.executor.SetFloat("DistVelocity", 0f);
						controller.executor.SetBool("Geometry Clampd", value: true);
					}
				}
				value.position = vector5 - value.nearClip * vector;
			}
			@float = Vector3.Distance(vector4, value.position);
			float num10 = num3;
			Vector3 vector7 = Vector3.up + vector * 0.3f;
			if (Physics.SphereCast(new Ray(vector5, vector7.normalized), num3 - 0.02f, out var hitInfo2, num3 * 10f, layerMask, QueryTriggerInteraction.Ignore))
			{
				num10 = hitInfo2.distance * 0.95f;
			}
			float num11 = 0f;
			if (@float < value.nearClip * 2f)
			{
				float num12 = @float / (value.nearClip * 2f);
				num11 = Mathf.Sqrt(1.0001f - num12 * num12) * value.nearClip * 2f;
			}
			if (num11 > num10)
			{
				num11 = num10;
			}
			if (@bool)
			{
				num11 = 0f;
			}
			rise = Mathf.Lerp(rise, num11, 0.1f);
			value.position += vector7 * rise;
			controller.executor.SetFloat("Dist", @float);
			return value;
		}
		return Pose.Default;
	}

	private bool GetBool(string name)
	{
		return controller.executor.GetVar(name).value_b;
	}

	private float GetFloat(string name)
	{
		return controller.executor.GetVar(name).value_f;
	}

	private Vector3 GetPosition(string name)
	{
		Transform transform = CameraController.GetTransform(name);
		if (transform == null)
		{
			return Vector3.zero;
		}
		return transform.position;
	}
}
