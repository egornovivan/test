using UnityEngine;

namespace EVP;

public class VehicleTelemetry : MonoBehaviour
{
	public VehicleController target;

	public bool show = true;

	public bool gizmos;

	public GUIStyle style = new GUIStyle();

	private string m_telemetryText = string.Empty;

	private void OnEnable()
	{
		if (target == null)
		{
			target = GetComponent<VehicleController>();
		}
	}

	private void FixedUpdate()
	{
		if (target != null && show)
		{
			m_telemetryText = DoTelemetry();
		}
	}

	private void Update()
	{
		if (target != null && gizmos)
		{
			DrawGizmos();
		}
	}

	private void OnGUI()
	{
		if (target != null && show)
		{
			GUI.Box(new Rect(8f, 8f, 500f, 180f), "Telemetry");
			GUI.Label(new Rect(16f, 28f, 400f, 270f), m_telemetryText, style);
		}
	}

	private string DoTelemetry()
	{
		string text = $"V: {target.speed,5:0.0} m/s  {target.speed * 3.6f,5:0.0} km/h {target.speed * 2.237f,5:0.0} mph\n\n";
		float suspensionForce = 0f;
		WheelData[] wheelData = target.wheelData;
		foreach (WheelData wd in wheelData)
		{
			text += GetWheelTelemetry(wd, ref suspensionForce);
		}
		text += $"\n     ΣF:{suspensionForce,6:0.}  Perceived mass:{(0f - suspensionForce) / Physics.gravity.y,7:0.0}\n               Rigidbody mass:{target.cachedRigidbody.mass,7:0.0}\n";
		VehicleAudio component = target.GetComponent<VehicleAudio>();
		if (component != null)
		{
			text += $"\nAudio gear/rpm:{component.simulatedGear,2:0.} {component.simulatedEngineRpm,5:0.}";
		}
		if (target.debugText != string.Empty)
		{
			text = text + "\n\n" + target.debugText;
		}
		return text;
	}

	private string GetWheelTelemetry(WheelData wd, ref float suspensionForce)
	{
		string text = $"{wd.collider.gameObject.name,-10}:{wd.angularVelocity * VehicleController.WToRpm,5:0.} rpm  ";
		if (wd.grounded)
		{
			text += $"C:{wd.suspensionCompression,5:0.00}  F:{wd.downforce,5:0.}  ";
			text += $"Sx:{wd.tireSlip.x,6:0.00} Sy:{wd.tireSlip.y,6:0.00}  ";
			text += $"Fx:{wd.tireForce.x,5:0.} Fy:{wd.tireForce.y,5:0.}  ";
			suspensionForce += wd.hit.force;
		}
		else
		{
			text += $"C: 0.--  ";
		}
		return text + "\n";
	}

	private void DrawGizmos()
	{
		CommonTools.DrawCrossMark(target.cachedTransform.TransformPoint(target.cachedRigidbody.centerOfMass), target.cachedTransform, Color.white);
		WheelData[] wheelData = target.wheelData;
		foreach (WheelData wd in wheelData)
		{
			DrawWheelGizmos(wd);
		}
		Vector3 pos = target.cachedTransform.TransformPoint(target.cachedRigidbody.centerOfMass + Vector3.forward * target.aeroAppPointOffset);
		CommonTools.DrawCrossMark(pos, target.cachedTransform, Color.cyan);
	}

	private void DrawWheelGizmos(WheelData wd)
	{
		if (wd.grounded && Physics.Raycast(wd.transform.position, -wd.transform.up, out var hitInfo, wd.collider.suspensionDistance + wd.collider.radius))
		{
			Debug.DrawLine(hitInfo.point, hitInfo.point + wd.transform.up * (wd.downforce / 10000f), (!(wd.suspensionCompression > 0.99f)) ? Color.white : Color.magenta);
			CommonTools.DrawCrossMark(wd.transform.position, wd.transform, Color.Lerp(Color.green, Color.gray, 0.5f));
			Vector3 vector = hitInfo.point + wd.transform.up * target.antiRoll * wd.forceDistance;
			CommonTools.DrawCrossMark(vector, wd.transform, Color.Lerp(Color.yellow, Color.gray, 0.5f));
			Vector3 val = wd.hit.forwardDir * wd.tireForce.y + wd.hit.sidewaysDir * wd.tireForce.x;
			Debug.DrawLine(vector, vector + CommonTools.Lin2Log(val) * 0.1f, Color.green);
			Vector3 val2 = wd.hit.forwardDir * wd.tireSlip.y + wd.hit.sidewaysDir * wd.tireSlip.x;
			Debug.DrawLine(hitInfo.point, hitInfo.point + CommonTools.Lin2Log(val2) * 0.5f, Color.cyan);
		}
	}
}
