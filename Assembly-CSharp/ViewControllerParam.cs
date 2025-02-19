public struct ViewControllerParam
{
	public float camNearClip;

	public float camFarClip;

	public float camAspect;

	public float camFieldOfView;

	public bool orthographic;

	public float orthographicSize;

	public int texWidth;

	public int texHeight;

	public static ViewControllerParam DefaultCharacter
	{
		get
		{
			ViewControllerParam result = default(ViewControllerParam);
			result.camNearClip = 0.1f;
			result.camFarClip = 10f;
			result.orthographic = false;
			result.camFieldOfView = 45f;
			result.texWidth = 620;
			result.texHeight = 960;
			result.camAspect = (float)result.texWidth / (float)result.texHeight;
			return result;
		}
	}

	public static ViewControllerParam DefaultPortrait
	{
		get
		{
			ViewControllerParam result = default(ViewControllerParam);
			result.camNearClip = 0f;
			result.camFarClip = 10f;
			result.camAspect = 1f;
			result.orthographic = true;
			result.orthographicSize = 0.175f;
			result.texWidth = 256;
			result.texHeight = 256;
			return result;
		}
	}

	public static ViewControllerParam DefaultSpicies
	{
		get
		{
			ViewControllerParam result = default(ViewControllerParam);
			result.camNearClip = 0.1f;
			result.camFarClip = 100f;
			result.orthographic = false;
			result.camFieldOfView = 45f;
			result.texWidth = 550;
			result.texHeight = 330;
			result.camAspect = (float)result.texWidth / (float)result.texHeight;
			return result;
		}
	}
}
