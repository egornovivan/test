using UnityEngine;

namespace WhiteCat;

public class BaseBehaviour : MonoBehaviour
{
	public RectTransform rectTransform => base.transform as RectTransform;
}
