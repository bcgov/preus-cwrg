using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class SuccessStoryService : Service, ISuccessStoryService
	{
		private readonly IGrantApplicationService _grantApplicationService;
		private readonly INoteService _noteService;

		public SuccessStoryService(IGrantApplicationService grantApplicationService,
			INoteService noteService,
			IDataContext context,
			HttpContextBase httpContext,
			ILogger logger)
			: base(context, httpContext, logger)
		{
			_grantApplicationService = grantApplicationService;
			_noteService = noteService;
		}

		public SuccessStory GetSuccessStory(int grantApplicationId)
		{
			var grantApplication = _grantApplicationService.Get(grantApplicationId);
			var successStory = _dbContext.SuccessStories.FirstOrDefault(ss => ss.GrantApplicationId == grantApplication.Id);

			return successStory;
		}

		public void AddSuccessStory(SuccessStory successStory)
		{
			if (successStory.Id == 0)
				_dbContext.SuccessStories.Add(successStory);

			CommitTransaction();
		}

		public void UpdateSuccessStory(SuccessStory successStory)
		{
			if (successStory.Id == 0)
				_dbContext.SuccessStories.Add(successStory);

			if (_httpContext.User.GetAccountType() == AccountTypes.Internal)
				_noteService.GenerateUpdateNote(successStory.GrantApplication);

			_dbContext.Update(successStory);

			CommitTransaction();
		}

		public void ToggleSuccessStory(GrantApplication grantApplication)
		{
			var successStory = GetSuccessStory(grantApplication.Id);

			// Shouldn't really be happening
			if (successStory == null)
			{
				successStory = new SuccessStory
				{
					GrantApplicationId = grantApplication.Id,
					State = SuccessStoryState.Incomplete,
					Documents = new List<Attachment>(),
					RowVersion = grantApplication.RowVersion
				};

				AddSuccessStory(successStory);
			}

			successStory.State = successStory.State == SuccessStoryState.Incomplete
				? SuccessStoryState.Complete
				: SuccessStoryState.Incomplete;
			successStory.DateUpdated = AppDateTime.UtcNow;

			_dbContext.Update(successStory);

			CommitTransaction();
		}
	}
}