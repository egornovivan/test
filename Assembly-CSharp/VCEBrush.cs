using UnityEngine;

public abstract class VCEBrush : MonoBehaviour
{
	public EVCEBrushType m_Type = EVCEBrushType.General;

	protected VCEAction m_Action;

	protected abstract void Do();

	public virtual void Cancel()
	{
	}
}
