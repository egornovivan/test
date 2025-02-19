using Steamworks;

public class SteamPreFileAndVoteDetail
{
	public PublishedFileId_t m_nPublishedFileId;

	public UGCHandle_t m_hFile;

	public UGCHandle_t m_hPreviewFile;

	public string m_pchFileName;

	public byte[] m_aPreFileData;

	public string m_sUploader;

	public string m_rgchTitle;

	public string m_rgchDescription;

	public SteamVoteDetail m_VoteDetail;

	public string m_rgchTags;

	public SteamPreFileAndVoteDetail()
	{
		m_VoteDetail = new SteamVoteDetail();
	}
}
