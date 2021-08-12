using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using CJG.Core.Interfaces.Service;
using CJG.Core.Interfaces.Service.Settings;
using NLog;

namespace CJG.Application.Services
{
	/// <summary>
	/// EmailSenderService class, provides a way to send email messages to the SMTP server.
	/// </summary>
	public class EmailSenderService : IEmailSenderService
	{
		private readonly IEmailSenderSettings _settings;

		private readonly ILogger _logger;

		/// <summary>
		/// Creates a new instance of a EmailSenderService object, and initializes it with the specified arguments.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public EmailSenderService(IEmailSenderSettings settings, ILogger logger)
		{
			_settings = settings;
			_logger = logger;
		}

		/// <summary>
		/// Sends an email to the specified recipient.
		/// </summary>
		/// <param name="recipient"></param>
		/// <param name="subject"></param>
		/// <param name="body"></param>
		/// <param name="senderName"></param>
		/// <param name="senderAddress"></param>
		public void Send(string recipient, string subject, string body, string senderName, string senderAddress)
		{
			if (string.IsNullOrWhiteSpace(recipient))
				throw new ArgumentException($"Argument {nameof(recipient)} is required.");

			Send(new []{ recipient }, subject, body, senderName, senderAddress);
		}

		/// <summary>
		/// Sends an email to the specified recipients.
		/// </summary>
		/// <param name="recipients"></param>
		/// <param name="subject"></param>
		/// <param name="body"></param>
		/// <param name="senderName"></param>
		/// <param name="senderAddress"></param>
		public void Send(IEnumerable<string> recipients, string subject, string body, string senderName, string senderAddress)
		{
			if (recipients == null || recipients.Count() == 0)
				throw new ArgumentException($"Argument {nameof(recipients)} is required.", nameof(recipients));
			if (string.IsNullOrWhiteSpace(subject))
				throw new ArgumentException($"Argument {nameof(subject)} is required.", nameof(subject));
			if (string.IsNullOrWhiteSpace(body))
				throw new ArgumentException($"Argument {nameof(body)} is required.", nameof(body));
			if (string.IsNullOrWhiteSpace(senderAddress))
				throw new ArgumentException($"Argument {nameof(senderAddress)} is required.", nameof(senderAddress));

			try
			{
				using (var email = new MailMessage())
				{
					email.From = new MailAddress(senderAddress, senderName);

					foreach (var recipient in recipients)
					{
						email.To.Add(new MailAddress(recipient));    
					}       
				
					email.Subject = subject;
					email.Body = body;
					email.IsBodyHtml = true;

					using (SmtpClient smtpClient = new SmtpClient(_settings.SmtpServer) { Timeout = (int) _settings.SendTimeout.TotalMilliseconds})
					{
						smtpClient.Send(email);
					}
				}
			}
			catch (Exception e)
			{
				_logger.Error(e, $"Unable to send email, recipient: {String.Join("; ", recipients)}, subject: {subject}, sender name: {senderName}, sender address: {senderAddress}");
				throw;
			}
		}
	}
}