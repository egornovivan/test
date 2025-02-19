public class CameraPlot
{
	public int m_ID;

	public int m_ControlType;

	public CameraMove m_CamMove;

	public CameraRot m_CamRot;

	public CameraTrack m_CamTrack;

	public bool m_CamToPlayer;

	public int m_Delay;

	public CameraPlot()
	{
		m_CamMove = new CameraMove();
		m_CamRot = new CameraRot();
		m_CamTrack = new CameraTrack();
	}
}
