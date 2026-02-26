using System.Linq;
using System.Web;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	public class AttestationService : Service, IAttestationService
	{
		public AttestationService(
			IDataContext context,
			HttpContextBase httpContext,
			ILogger logger) : base(context, httpContext, logger)
		{
		}

		public void DeleteExistingAttestationParticipant(int grantApplicationId)
		{
			var costs = _dbContext.AttestationParticipantCosts.Where(apc => apc.AttestationParticipant.ParticipantForm.GrantApplicationId == grantApplicationId);
			var participants = _dbContext.AttestationParticipants.Where(ap => ap.ParticipantForm.GrantApplicationId == grantApplicationId);

			_dbContext.AttestationParticipantCosts.RemoveRange(costs);
			_dbContext.AttestationParticipants.RemoveRange(participants);

			_dbContext.SaveChanges();
		}
	}
}