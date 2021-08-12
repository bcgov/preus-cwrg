using System;
using System.Collections.Generic;

namespace CJG.Application.Business.Models
{
	public class ApplicationAccountsReceivableUpdateModel
	{
		public int GrantApplicationId { get; set; }
		public DateTime PaidDate { get; set; }

		public List<KeyValuePair<int, decimal>> ReceivablesByServiceCategory = new List<KeyValuePair<int,decimal>>();
	}
}