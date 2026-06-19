using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class ParticipantFundingStreamService : Service, IParticipantFundingStreamService
	{
		public ParticipantFundingStreamService(IDataContext context, HttpContextBase httpContext, ILogger logger)
			: base(context, httpContext, logger)
		{
		}

		public ParticipantFundingStream Get(int id)
		{
			return Get<ParticipantFundingStream>(id);
		}

		public IEnumerable<ParticipantFundingStream> GetAll()
		{
			return _dbContext.ParticipantFundingStreams
				.OrderBy(pi => pi.RowSequence);
		}

		public IEnumerable<ParticipantFundingStream> GetAll(bool isActive)
		{
			var fundingStreams = _dbContext.ParticipantFundingStreams
				.AsNoTracking()
				.Where(pi => pi.IsActive == isActive)
				.OrderBy(pi => pi.RowSequence)
				.AsQueryable();

			return fundingStreams.ToList();
		}

		public bool IsInUse(int participantFundingStreamId)
		{
			var reservedFundingStreams = new List<int> { (int) ParticipantFundingStreams.ComprehensiveTariffs, (int) ParticipantFundingStreams.NotApplicable };
			var isReservedStream = reservedFundingStreams.Contains(participantFundingStreamId);

			return isReservedStream || _dbContext.ParticipantForms.Any(ap => ap.ParticipantFundingStreamId == participantFundingStreamId);
		}

		public void UpdateFundingStreams(List<ParticipantFundingStream> streamList, List<ParticipantFundingStream> removeItems)
		{
			if (streamList == null)
				throw new ArgumentNullException(nameof(streamList));

			foreach (var fundingStream in streamList)
			{
				if (fundingStream.Id == 0)
					_dbContext.ParticipantFundingStreams.Add(fundingStream);
			}

			// Have to explicitly remove these from the dbContext so they get deleted rather than disconnected
			foreach (var removeItem in removeItems)
			{
				if (IsInUse(removeItem.Id))
					continue;

				_dbContext.ParticipantFundingStreams.Remove(removeItem);
			}

			_dbContext.CommitTransaction();
		}
	}
}