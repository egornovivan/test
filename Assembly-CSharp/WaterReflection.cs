using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaterReflection : MonoBehaviour
{
	public enum Quality
	{
		Fastest,
		Medium,
		High
	}

	public static int ReflectionSetting = 1;

	public static HashSet<WaterReflection> InstanceSets;

	public Color BackgroundColor;

	public LayerMask MediumReflectionMask;

	public LayerMask HighReflectionMask;

	public Vector3 PlanePos = new Vector3(0f, 97f, 0f);

	public Vector3 PlaneNormal = Vector3.up;

	private RenderTexture m_Texture;

	private Camera m_ReflectionCam;

	private bool m_EnableRefl;

	private Camera m_CurCam;

	private bool bEnableWaterRef = true;

	public float Height
	{
		get
		{
			return PlanePos.y;
		}
		set
		{
			PlanePos.y = value;
		}
	}

	public Camera CurCam => m_CurCam;

	private Camera GetReflectionCamera(Camera current, int textureSize)
	{
		if (m_Texture == null)
		{
			m_Texture = new RenderTexture(textureSize, textureSize, 16);
			m_Texture.name = "_ReflectionTex" + GetInstanceID();
			m_Texture.isPowerOfTwo = true;
			m_Texture.hideFlags = HideFlags.DontSave;
		}
		if (m_CurCam != current)
		{
			if (m_ReflectionCam != null)
			{
				Object.Destroy(m_ReflectionCam.gameObject);
			}
			m_ReflectionCam = null;
			m_CurCam = current;
		}
		if (m_ReflectionCam == null)
		{
			GameObject gameObject = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + current.GetInstanceID(), typeof(Camera), typeof(Skybox));
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			m_ReflectionCam = gameObject.GetComponent<Camera>();
			m_ReflectionCam.enabled = false;
			Transform transform = m_ReflectionCam.transform;
			transform.position = base.transform.position;
			transform.rotation = base.transform.rotation;
		}
		return m_ReflectionCam;
	}

	private void CopyCameraSetting(Camera src, Camera dest)
	{
		if (src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox component = src.GetComponent<Skybox>();
			Skybox component2 = dest.GetComponent<Skybox>();
			if (!component || !component.material)
			{
				component2.enabled = false;
			}
			else
			{
				component2.enabled = true;
				component2.material = component.material;
			}
		}
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = BackgroundColor;
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
		dest.depthTextureMode = DepthTextureMode.Depth;
		dest.renderingPath = RenderingPath.Forward;
	}

	public static bool ReqRefl()
	{
		return ReflectionSetting != 0 && VFVoxelWater.s_bSeaInSight;
	}

	public static void DisableRefl()
	{
		foreach (WaterReflection instanceSet in InstanceSets)
		{
			if (instanceSet != null)
			{
				instanceSet.m_EnableRefl = false;
			}
		}
	}

	public static void EnableRefl()
	{
		foreach (WaterReflection instanceSet in InstanceSets)
		{
			if (instanceSet != null)
			{
				instanceSet.m_EnableRefl = true;
			}
		}
	}

	public static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(sgn(clipPlane.x), sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
		return projection;
	}

	public static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
		return reflectionMat;
	}

	private static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(pos);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	private void Clear()
	{
		if (m_Texture != null)
		{
			Object.DestroyImmediate(m_Texture);
			m_Texture = null;
		}
		if (VFVoxelWater.self != null && (bool)VFVoxelWater.self.WaterMat)
		{
			VFVoxelWater.self.WaterMat.SetTexture("_ReflectionTex", null);
		}
	}

	private void Awake()
	{
		if (InstanceSets == null)
		{
			InstanceSets = new HashSet<WaterReflection>();
		}
		InstanceSets.Add(this);
	}

	private void Start()
	{
		m_EnableRefl = false;
		Height = VFVoxelWater.c_fWaterLvl;
	}

	private void Update()
	{
		if (VFVoxelWater.self != null && (bool)VFVoxelWater.self.WaterMat)
		{
			VFVoxelWater.self.WaterMat.SetTexture("_WaveMap", PEWaveSystem.Self.Target);
		}
		if (Input.GetKey(KeyCode.Alpha0))
		{
			bEnableWaterRef = !bEnableWaterRef;
		}
	}

	private void OnDestroy()
	{
		Clear();
		if (m_ReflectionCam != null)
		{
			Object.DestroyImmediate(m_ReflectionCam.gameObject);
		}
		InstanceSets.Remove(this);
	}

	private void OnDisable()
	{
		if (m_ReflectionCam != null)
		{
			Object.DestroyImmediate(m_ReflectionCam.gameObject);
		}
	}

	private void OnPreRender()
	{
		if (VFVoxelWater.self == null || VFVoxelWater.self.WaterMat == null || !m_EnableRefl)
		{
			return;
		}
		Camera current = Camera.current;
		LayerMask highReflectionMask = HighReflectionMask;
		if (current == null || !base.enabled || (int)highReflectionMask == 0)
		{
			Clear();
			return;
		}
		if (SystemInfo.supportsImageEffects)
		{
			current.depthTextureMode |= DepthTextureMode.Depth;
		}
		Camera reflectionCamera = GetReflectionCamera(current, 512);
		reflectionCamera.enabled = false;
		if (VFVoxelWater.self != null && (bool)VFVoxelWater.self.WaterMat)
		{
			VFVoxelWater.self.WaterMat.SetTexture("_ReflectionTex", m_Texture);
		}
		CopyCameraSetting(current, reflectionCamera);
		Vector3 planePos = PlanePos;
		Vector3 planeNormal = PlaneNormal;
		float w = 0f - Vector3.Dot(planeNormal, planePos);
		Vector4 plane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, w);
		Matrix4x4 zero = Matrix4x4.zero;
		zero = CalculateReflectionMatrix(zero, plane);
		Vector3 position = current.transform.position;
		Vector3 position2 = zero.MultiplyPoint(position);
		reflectionCamera.worldToCameraMatrix = current.worldToCameraMatrix * zero;
		Vector4 clipPlane = CameraSpacePlane(reflectionCamera, planePos, planeNormal, 1f);
		Matrix4x4 zero2 = Matrix4x4.zero;
		zero2 = CalculateObliqueMatrix(current.projectionMatrix, clipPlane);
		reflectionCamera.projectionMatrix = zero2;
		reflectionCamera.cullingMask = -17 & highReflectionMask.value;
		reflectionCamera.targetTexture = m_Texture;
		GL.invertCulling = true;
		reflectionCamera.transform.position = position2;
		Vector3 eulerAngles = current.transform.eulerAngles;
		reflectionCamera.transform.eulerAngles = new Vector3(0f, eulerAngles.y, eulerAngles.z);
		reflectionCamera.Render();
		reflectionCamera.transform.position = position;
		GL.invertCulling = false;
	}
}
