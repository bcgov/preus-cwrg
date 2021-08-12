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
	public class LogService : Service, ILogService
	{
		public LogService(IDataContext context, HttpContextBase httpContext, ILogger logger) : base(context, httpContext, logger)
		{
		}

		public Log Get(int id)
		{
			if (id <= 0)
				return null;

			return _dbContext.Logs.Find(id);
		}

		public IEnumerable<Log> GetLogs(int page = 1, int numberOfItems = 25, string level = "*")
		{
			if (page < 1)
				page = 1;

			if (numberOfItems < 1)
				numberOfItems = 25;

			return _dbContext.Logs
				.AsNoTracking()
				.Where(l => (level == "*" || l.Level == level))
				.OrderByDescending(l => l.DateAdded)
				.Skip((page - 1) * numberOfItems)
				.Take(numberOfItems)
				.ToArray();
		}

		public IEnumerable<Log> Filter(string level = "*", DateTime? dateAdded = null, string message = null, string userName = null, bool excludeLoginMessages = false, int page = 1, int numberOfItems = 25)
		{
			if (page < 1) page = 1;
			if (numberOfItems < 1) numberOfItems = 25;

			var logs = _dbContext.Logs
				.AsNoTracking()
				.Where(l => (level == "*" || l.Level == level)
				            && (dateAdded == null || l.DateAdded >= dateAdded)
				            && (message == null || l.Message.Contains(message))
				            && (userName == null || l.UserName.Contains(userName))
				).AsQueryable();
//				&& (excludeLoginMessages 
			if (excludeLoginMessages)
				logs = logs.Where(l => !(l.Message.Contains("login")
				                       || l.Message.Contains("logout")
				                       || l.Message.StartsWith("siteminder")
				                       || l.Message.StartsWith("cjg")));
			
			logs = logs.OrderByDescending(l => l.DateAdded)
				.Skip((page - 1) * numberOfItems)
				.Take(numberOfItems);

			return logs.ToArray();
		}

		public void Add(Log log)
		{
			_dbContext.Logs.Add(log);
			Commit();
		}

		public void Update(Log log)
		{
			_dbContext.Update<Log>(log);
			Commit();
		}

		public void Delete(Log log)
		{
			_dbContext.Logs.Remove(log);
			_dbContext.Commit();
		}

		public void Delete(DateTime before)
		{
			var logs = _dbContext.Logs.AsNoTracking().Where(l => l.DateAdded < before);
			foreach (var log in logs)
			{
				_dbContext.Logs.Remove(log);
			}
			_dbContext.Commit();
		}
	}
}
