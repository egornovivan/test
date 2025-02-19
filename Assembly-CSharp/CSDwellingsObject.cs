using UnityEngine;

public class CSDwellingsObject : CSEntityObject
{
	public CSDwellingsInfo m_Info;

	public Transform[] m_BedTrans;

	public Transform[] m_BedEdgeTrans;

	public CSDwellings m_Dwellings => (m_Entity != null) ? (m_Entity as CSDwellings) : null;

	private new void Start()
	{
		base.Start();
		m_Info = CSInfoMgr.m_DwellingsInfo;
	}

	private new void Update()
	{
		base.Update();
	}
}
