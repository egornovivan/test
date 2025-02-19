using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class BugReporter
{
	private static bool _isAlreadyRun;

	private static int _maxRetryCount = 5;

	private static int _currentRetryCount;

	private static MailMessage _mailMessage;

	private static FileStream _fsAttachment;

	private static Action _onFinSend;

	public static bool IsSending => _isAlreadyRun;

	public static void SendEmailAsync(string msg, int retryCount, Action onFinSend = null)
	{
		if (_isAlreadyRun)
		{
			Debug.LogError("EmailSender doesn't support multiple concurrent invocations.");
			return;
		}
		string text = "Bug report";
		text += " Win64";
		text = text + " Steam Version:" + GameConfig.GameVersion;
		try
		{
			text = text + " from SteamId " + SteamMgr.steamId.m_SteamID;
		}
		catch
		{
			text += " from Unrecognized SteamId";
		}
		_isAlreadyRun = true;
		_onFinSend = onFinSend;
		_maxRetryCount = retryCount;
		_mailMessage = new MailMessage();
		_mailMessage.From = new MailAddress("pe.bugreport@pathea.net");
		_mailMessage.To.Add("pe.bugreport@pathea.net");
		_mailMessage.Subject = text;
		_mailMessage.Body = msg;
		_mailMessage.IsBodyHtml = false;
		_mailMessage.Priority = MailPriority.High;
		string text2 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/..";
		try
		{
			_fsAttachment = new FileStream(Application.dataPath + "/output_log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			_mailMessage.Attachments.Add(new Attachment(_fsAttachment, "output_log.txt"));
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to attach:" + ex);
		}
		SmtpClient smtpClient = new SmtpClient("pathea.net");
		smtpClient.Port = 587;
		smtpClient.Credentials = new NetworkCredential("pe.bugreport@pathea.net", "mailbox4pebugreport");
		smtpClient.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = (object P_0, X509Certificate P_1, X509Chain P_2, SslPolicyErrors P_3) => true;
		smtpClient.SendCompleted += SmtpClientSendCompleted;
		SendEmail(smtpClient);
	}

	private static void SendEmail(SmtpClient smtpServer)
	{
		try
		{
			if (_maxRetryCount > 0)
			{
				smtpServer.SendAsync(_mailMessage, Guid.NewGuid());
				return;
			}
			smtpServer.Send(_mailMessage);
			EndProcessing(smtpServer);
			Debug.Log("Succeed");
		}
		catch (Exception ex)
		{
			EndProcessing(smtpServer);
			Debug.Log("Failed:" + ex);
		}
	}

	private static void EndProcessing(SmtpClient client)
	{
		if (_mailMessage != null)
		{
			_mailMessage.Dispose();
		}
		if (_fsAttachment != null)
		{
			_fsAttachment.Dispose();
		}
		if (client != null)
		{
			client.SendCompleted -= SmtpClientSendCompleted;
		}
		if (_onFinSend != null)
		{
			_onFinSend();
		}
		_isAlreadyRun = false;
		_currentRetryCount = 0;
	}

	private static void SmtpClientSendCompleted(object sender, AsyncCompletedEventArgs e)
	{
		SmtpClient smtpClient = (SmtpClient)sender;
		if (e.Error == null || _currentRetryCount >= _maxRetryCount)
		{
			EndProcessing(smtpClient);
			return;
		}
		_currentRetryCount++;
		SendEmail(smtpClient);
	}
}
