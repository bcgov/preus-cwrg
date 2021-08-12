using System;
using System.Linq;
using System.Web;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using NLog;

namespace CJG.Application.Services
{
	/// <summary>
	/// <typeparamref name="DenialReasonService"/> class, provides a way to manage notifications types.
	/// </summary>
	public class DenialReasonService : Service, IDenialReasonService
	{
		/// <summary>
		/// Creates a new instance of a <typeparamref name="NotificationTypeService"/> class.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="httpContext"></param>
		/// <param name="logger"></param>
		public DenialReasonService(
			IDataContext dbContext,
			HttpContextBase httpContext,
			ILogger logger) : base(dbContext, httpContext, logger)
		{
		}

		/// <summary>
		/// Returns an array of notification types filtered by 'isActive' argument.
		/// </summary>
		/// <param name="isActive"></param>
		/// <returns></returns>
		public DenialReason Get(bool? isActive)
		{
			return (DenialReason)_dbContext.DenialReasons.Where(nt => nt.IsActive == (isActive ?? true));
		}

		/// <summary>
		/// Return the Denial Reason for the specified 'id' or throw NoContentException if not found.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public DenialReason Get(int Id)
		{
			return Get<DenialReason>(Id);
		}

		public void Add(DenialReason denialReason)
		{
			if (denialReason == null)
				throw new ArgumentNullException(nameof(denialReason));

			_dbContext.DenialReasons.Add(denialReason);

			_dbContext.CommitTransaction();
		}
	}
}
