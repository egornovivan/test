using System.Collections;
using UnityEngine;

namespace UnitySteer;

public class lqBin
{
	public ArrayList clientList;

	public Vector3 center;

	public lqBin(Vector3 binCenter)
	{
		clientList = new ArrayList();
		center = binCenter;
	}
}
