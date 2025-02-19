using UnityEngine;

public class ViewController : MonoBehaviour
{
	public int ID;

	[SerializeField]
	protected Camera _viewCam;

	protected RenderTexture m_RenderTex;

	protected Transform m_Target;

	[SerializeField]
	protected Vector3 m_Offset;

	protected Quaternion m_Rot = Quaternion.identity;

	public Camera viewCam => _viewCam;

	public RenderTexture RenderTex => m_RenderTex;

	public Transform target => m_Target;

	public void SetLocalTrans(Vector3 pos, Quaternion rot)
	{
		SetLocalPos(pos);
		SetLocalRot(rot);
	}

	public void SetLocalPos(Vector3 pos)
	{
		m_Offset = pos;
	}

	public void SetLocalRot(Quaternion rot)
	{
		m_Rot = rot;
	}

	public void Set(Transform target, ViewControllerParam param)
	{
		SetTarget(target);
		SetParam(param);
	}

	public virtual void SetTarget(Transform target)
	{
		m_Target = target;
	}

	public virtual void SetParam(ViewControllerParam param)
	{
		if (m_RenderTex == null)
		{
			m_RenderTex = new RenderTexture(param.texWidth, param.texHeight, 16, RenderTextureFormat.ARGB32);
			m_RenderTex.isCubemap = false;
		}
		else if (m_RenderTex.width != param.texWidth || m_RenderTex.height == param.texHeight)
		{
			m_RenderTex.Release();
			Object.Destroy(m_RenderTex);
			m_RenderTex = new RenderTexture(param.texWidth, param.texHeight, 16, RenderTextureFormat.ARGB32);
			m_RenderTex.isCubemap = false;
		}
		_viewCam.targetTexture = m_RenderTex;
		_viewCam.nearClipPlane = param.camNearClip;
		_viewCam.farClipPlane = param.camFarClip;
		_viewCam.aspect = param.camAspect;
		_viewCam.fieldOfView = param.camFieldOfView;
		_viewCam.orthographic = param.orthographic;
		_viewCam.orthographicSize = param.orthographicSize;
	}

	public Texture2D GetTexture2D()
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = _viewCam.targetTexture;
		_viewCam.Render();
		Texture2D texture2D = new Texture2D(_viewCam.targetTexture.width, _viewCam.targetTexture.height, TextureFormat.ARGB32, mipmap: false);
		texture2D.ReadPixels(new Rect(0f, 0f, _viewCam.targetTexture.width, _viewCam.targetTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		return texture2D;
	}
}
