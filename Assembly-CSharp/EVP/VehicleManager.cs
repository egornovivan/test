using UnityEngine;

namespace EVP;

public class VehicleManager : MonoBehaviour
{
	public VehicleController[] vehicles = new VehicleController[0];

	public int defaultVehicle;

	public KeyCode previousVehicleKey = KeyCode.PageUp;

	public KeyCode nextVehicleKey = KeyCode.PageDown;

	public VehicleCameraController cameraController;

	public bool overrideVehicleComponents = true;

	private int m_currentVehicleIdx = -1;

	private VehicleController m_currentVehicle;

	private VehicleStandardInput m_commonInput;

	private VehicleTelemetry m_commonTelemetry;

	private void OnEnable()
	{
		m_commonInput = GetComponent<VehicleStandardInput>();
		m_commonTelemetry = GetComponent<VehicleTelemetry>();
	}

	private void Start()
	{
		VehicleController[] array = vehicles;
		foreach (VehicleController vehicle in array)
		{
			DisableVehicle(vehicle);
		}
		SelectVehicle(defaultVehicle);
	}

	private void Update()
	{
		if (Input.GetKeyDown(previousVehicleKey))
		{
			SelectPreviousVehicle();
		}
		if (Input.GetKeyDown(nextVehicleKey))
		{
			SelectNextVehicle();
		}
	}

	public void SelectVehicle(int vehicleIdx)
	{
		if (vehicleIdx <= vehicles.Length)
		{
			if (m_currentVehicle != null)
			{
				DisableVehicle(m_currentVehicle);
				m_currentVehicle = null;
			}
			if (vehicleIdx >= 0)
			{
				m_currentVehicle = vehicles[vehicleIdx];
				EnableVehicle(m_currentVehicle);
			}
			m_currentVehicleIdx = vehicleIdx;
		}
	}

	public void SelectPreviousVehicle()
	{
		int num = m_currentVehicleIdx - 1;
		if (num < 0)
		{
			num = vehicles.Length - 1;
		}
		if (num >= 0)
		{
			SelectVehicle(num);
		}
	}

	public void SelectNextVehicle()
	{
		int num = m_currentVehicleIdx + 1;
		if (num >= vehicles.Length)
		{
			num = 0;
		}
		SelectVehicle((num >= vehicles.Length) ? (-1) : num);
	}

	private void EnableVehicle(VehicleController vehicle)
	{
		if (vehicle == null)
		{
			return;
		}
		SetupVehicleComponents(vehicle, enabled: true);
		if (cameraController != null)
		{
			VehicleViewConfig component = vehicle.GetComponent<VehicleViewConfig>();
			if (component != null)
			{
				cameraController.target = ((!(component.lookAtPoint != null)) ? vehicle.transform : component.lookAtPoint);
				cameraController.targetFixedPosition = component.driverView;
				cameraController.smoothFollowSettings.distance = component.viewDistance;
				cameraController.smoothFollowSettings.height = component.viewHeight;
				cameraController.smoothFollowSettings.rotationDamping = component.viewDamping;
				cameraController.orbitSettings.distance = component.viewDistance;
				cameraController.orbitSettings.minDistance = component.viewMinDistance;
				cameraController.orbitSettings.minVerticalAngle = component.viewMinHeight;
			}
			else
			{
				cameraController.target = vehicle.transform;
			}
			cameraController.ResetCamera();
		}
	}

	private void DisableVehicle(VehicleController vehicle)
	{
		if (!(vehicle == null))
		{
			SetupVehicleComponents(vehicle, enabled: false);
			vehicle.throttleInput = 0f;
			vehicle.brakeInput = 1f;
		}
	}

	private void SetupVehicleComponents(VehicleController vehicle, bool enabled)
	{
		VehicleTelemetry component = vehicle.GetComponent<VehicleTelemetry>();
		VehicleStandardInput component2 = vehicle.GetComponent<VehicleStandardInput>();
		VehicleDamage component3 = vehicle.GetComponent<VehicleDamage>();
		if (component2 != null)
		{
			if (m_commonInput != null)
			{
				if (overrideVehicleComponents)
				{
					component2.enabled = false;
					m_commonInput.enabled = true;
					m_commonInput.target = ((!enabled) ? null : vehicle);
				}
				else
				{
					component2.enabled = enabled;
					m_commonInput.enabled = false;
				}
			}
			else
			{
				component2.enabled = enabled;
			}
		}
		else if (m_commonInput != null)
		{
			m_commonInput.enabled = true;
			m_commonInput.target = ((!enabled) ? null : vehicle);
		}
		if (component != null)
		{
			if (m_commonTelemetry != null)
			{
				if (overrideVehicleComponents)
				{
					component.enabled = false;
					m_commonTelemetry.enabled = true;
					m_commonTelemetry.target = ((!enabled) ? null : vehicle);
				}
				else
				{
					component.enabled = enabled;
					m_commonTelemetry.enabled = false;
				}
			}
			else
			{
				component.enabled = enabled;
			}
		}
		else if (m_commonTelemetry != null)
		{
			m_commonTelemetry.enabled = true;
			m_commonTelemetry.target = ((!enabled) ? null : vehicle);
		}
		if (component3 != null)
		{
			component3.enableRepairKey = enabled;
		}
	}
}
