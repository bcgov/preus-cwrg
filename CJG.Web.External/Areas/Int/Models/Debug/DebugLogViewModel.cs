using System;
using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Web.External.Areas.Int.Models.Debug
{
    public class DebugLogViewModel
    {
	    public int Page { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 50;
        public int Total { get; set; }
        public IEnumerable<Log> Logs { get; set; }

        public string Level { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public DateTime? DateAdded { get; set; }
        public bool ExcludeAuthMessages { get; set; }

        public DebugLogViewModel()
        {

        }

        public DebugLogViewModel(int page, int itemsPerPage, IEnumerable<Log> logs, int total)
        {
            Page = page;
            ItemsPerPage = itemsPerPage;
            Logs = logs;
            Total = total;
        }
    }
}