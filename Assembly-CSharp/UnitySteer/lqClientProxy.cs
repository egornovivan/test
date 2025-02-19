namespace UnitySteer;

public class lqClientProxy
{
	public lqBin bin;

	public object clientObject;

	public float x;

	public float y;

	public float z;

	public lqClientProxy(object tClientObject)
	{
		clientObject = tClientObject;
		x = 0f;
		y = 0f;
		z = 0f;
	}
}
