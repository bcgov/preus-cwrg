using System.Web;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class EvaluationNoteService : Service, IEvaluationNoteService
	{
		private readonly IStaticDataService _staticDataService;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="staticDataService"></param>
		/// <param name="context"></param>
		/// <param name="httpContext"></param>
		/// <param name="logger"></param>
		public EvaluationNoteService(IStaticDataService staticDataService, IDataContext context, HttpContextBase httpContext, ILogger logger) : base(context, httpContext, logger)
		{
			_staticDataService = staticDataService;
		}

		/// <summary>
		/// Get the note for the specified Id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public GrantApplicationEvaluationNote Get(int id)
		{
			var note = Get<GrantApplicationEvaluationNote>(id);
			Get<GrantApplication>(note.GrantApplicationId);

			return note;
		}

		/// <summary>
		/// Add the note to the datasource.
		/// </summary>
		/// <param name="note"></param>
		/// <returns></returns>
		public GrantApplicationEvaluationNote Add(GrantApplicationEvaluationNote note)
		{
			_dbContext.GrantApplicationEvaluationNotes.Add(note);
			_dbContext.CommitTransaction();
			return note;
		}

		/// <summary>
		/// Update the note in the datasource.
		/// </summary>
		/// <param name="note"></param>
		/// <returns></returns>
		public GrantApplicationEvaluationNote Update(GrantApplicationEvaluationNote note)
		{
			if (note.CreatorId != _httpContext.User.GetUserId() || note.NoteType.IsSystem)
			{
				throw new NotAuthorizedException($"User does not have permission to update note: '{note.Id}'.");
			}
			_dbContext.Update(note);
			_dbContext.CommitTransaction();

			return note;
		}

		/// <summary>
		/// Delete the note from the datasource.
		/// </summary>
		/// <param name="note"></param>
		public void Remove(GrantApplicationEvaluationNote note)
		{
			if (note.CreatorId != _httpContext.User.GetUserId() || note.NoteType.IsSystem)
			{
				throw new NotAuthorizedException($"User does not have permission to update note: '{note.Id}'.");
			}

			// Delete attachments.
			if (note.AttachmentId != null)
			{
				var attachment = Get<Attachment>(note.AttachmentId);
				attachment.Versions.ForEach(a => _dbContext.VersionedAttachments.Remove(a));
				_dbContext.Attachments.Remove(attachment);
			}

			_dbContext.GrantApplicationEvaluationNotes.Remove(note);
			_dbContext.CommitTransaction();
		}


		/// <summary>
		/// Create a Note object and initialize it.
		/// This does not add it to the datasource.
		/// </summary>
		/// <param name="grantApplication"></param>
		/// <param name="type"></param>
		/// <param name="message"></param>
		/// <param name="attachment"></param>
		/// <returns></returns>
		public GrantApplicationEvaluationNote CreateNote(GrantApplication grantApplication, NoteTypes type, string message, Attachment attachment = null)
		{
			var isInternal = _httpContext.User.GetAccountType() == AccountTypes.Internal;
			var user = isInternal ? Get<InternalUser>(_httpContext.User.GetUserId()) : null;
			var noteType = _staticDataService.GetNoteType(type);
			return new GrantApplicationEvaluationNote(grantApplication, noteType, message, attachment)
			{
				Creator = user
			};
		}
	}
}