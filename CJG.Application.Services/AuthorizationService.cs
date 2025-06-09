using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Security;
using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;
using CJG.Infrastructure.Entities;
using CJG.Infrastructure.Identity;
using Microsoft.AspNet.Identity;
using NLog;

namespace CJG.Application.Services
{
	public sealed class AuthorizationService : Service, IAuthorizationService
	{
		private readonly ApplicationUserManager _applicationUserManager;

		public AuthorizationService(ApplicationUserManager applicationUserManager, IDataContext context, HttpContextBase httpContext, ILogger logger) : base(context, httpContext, logger)
		{
			_applicationUserManager = applicationUserManager;
		}

		/// <summary>
		/// Return an array of internal users who are 'primary' assessors.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<InternalUser> GetPrimaryAssessors()
		{
			// This is currently the same logic as Assessors. Separate in case we need to split it.
			string[] includeRoles = { "Assessor", "Financial Clerk" };

			var roleIds = _dbContext.ApplicationClaims
				.SelectMany(x => x.ApplicationRoles)
				.Where(x => includeRoles.Contains(x.Name))
				.Select(x => x.Id)
				.Distinct();

			return _applicationUserManager.Users
				.Where(u => (u.Active ?? false) && u.Roles.Any(r => roleIds.Contains(r.RoleId)))
				.OrderBy(u => u.InternalUser.LastName)
				.ThenBy(u => u.InternalUser.FirstName)
				.Select(a => a.InternalUser).AsEnumerable();
		}

		/// <summary>
		/// Return an array of internal users who are assessors.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<InternalUser> GetAssessors()
		{
			string[] includeRoles = { "Assessor", "Financial Clerk" };

			var roleIds = _dbContext.ApplicationClaims
				.SelectMany(x => x.ApplicationRoles)
				.Where(x => includeRoles.Contains(x.Name))
				.Select(x => x.Id)
				.Distinct();

			return _applicationUserManager.Users
				.Where(u => (u.Active ?? false) && u.Roles.Any(r => roleIds.Contains(r.RoleId)))
				.OrderBy(u => u.InternalUser.LastName)
				.ThenBy(u => u.InternalUser.FirstName)
				.Select(a => a.InternalUser).AsEnumerable();
		}

		public IEnumerable<string> GetRolesWithPrivilege(string privilegeName)
		{
			try
			{
				return new HashSet<string>(_dbContext.ApplicationClaims.Where(c => c.ClaimType == AppClaimTypes.Privilege && c.ClaimValue == privilegeName)
					.SelectMany(x => x.ApplicationRoles)
					.Select(x => x.Id));
			}
			catch (Exception e)
			{
				_logger.Error(e, "Couldn't get priveleges for role: {0}", privilegeName);
				throw;
			}
		}

		public IEnumerable<string> GetPrivileges(string roleName)
		{
			try
			{
				return new HashSet<string>(_dbContext.ApplicationRoles.Where(x=>x.Name == roleName)
					.SelectMany(x=>x.ApplicationClaims)
					.Select(x=>x.ClaimValue));
			}
			catch (Exception e)
			{
				_logger.Error(e, "Couldn't get priveleges for role: {0}", roleName);
				throw;
			}
		}

		public void UpdatePrivilegeClaimsOnIdentity(ClaimsIdentity user)
		{
			try
			{
				var userName = user.GetUserName();
				var internalUser = _dbContext.InternalUsers.First(u => u.IDIR == userName);

				// Add the application Id for the user.
				user.AddClaim(new System.Security.Claims.Claim(AppClaimTypes.UserId, internalUser.Id.ToString(), ClaimValueTypes.Integer, "CJG"));
				user.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.GivenName, internalUser.FirstName, ClaimValueTypes.String, "CJG"));
				user.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Surname, internalUser.LastName, ClaimValueTypes.String, "CJG"));
				user.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, internalUser.Email, ClaimValueTypes.Email, "CJG"));
				user.AddClaim(new System.Security.Claims.Claim(AppClaimTypes.AccountType, AccountTypes.Internal.ToString(), ClaimValueTypes.String, "CJG"));

				var existingPrivilegeClaims = user.Claims
					.Where(c => c.Type == AppClaimTypes.Privilege);

				// Remove all existing privilege claims
				foreach (var privilegeClaim in existingPrivilegeClaims)
				{
					user.RemoveClaim(privilegeClaim);
				}

				// Collect privileges form roles
				var privileges = new HashSet<string>( GetUserRoles(user).SelectMany(GetPrivileges));
		
				// Add new claims from role's privilege collection
				foreach (var privilege in privileges)
				{
					user.AddClaim(new System.Security.Claims.Claim(AppClaimTypes.Privilege, privilege, ClaimValueTypes.String, "CJG")); 
				}
			}
			catch (Exception e)
			{
				_logger.Error(e, "Couldn't update privilege claims on identity: {0}", user.Name);
				throw;
			}
		}

		internal IEnumerable<string> GetUserRoles(ClaimsIdentity identity)
		{
			try
			{
				return identity.Claims
					.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
					.Select(c => c.Value).ToArray();
			}
			catch (Exception e)
			{
				_logger.Error(e, "Couldn't get user roles for identity: {0}", identity.Name);
				throw;
			}
		}
	}
}