using System;
using System.Collections.Generic;
using System.Linq;
using CJG.Application.Services.Exceptions;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Int.Models.Organizations
{
    public class OrganizationProfileUserListViewModel : BaseViewModel
	{
		public string OrganizationName { get; set; }
		public IEnumerable<OrganizationProfileUserViewModel> Users { get; set; }

		public OrganizationProfileUserListViewModel()
		{
		}

		public OrganizationProfileUserListViewModel(IUserService userService, IOrganizationService organizationService, int organizationId)
		{
			if (userService == null) throw new ArgumentNullException(nameof(userService));
			if (organizationService == null) throw new ArgumentNullException(nameof(organizationService));

			var users = userService.GetUsersForOrganization(organizationId);
			var organization = organizationService.Get(organizationId);

			OrganizationName = organization.LegalName;
			Users = users.Select(o =>
			{
				User user = o;
				try
				{
					user = userService.GetBCeIDUser(o.BCeIDGuid);
				}
				catch (NotAuthorizedException)
				{
					user.BCeID = "N/A";
				}
				return new OrganizationProfileUserViewModel(user);
			}).ToArray();
		}
	}
}
