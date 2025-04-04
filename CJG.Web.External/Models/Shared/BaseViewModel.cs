﻿using System.Collections.Generic;
using System.Linq;

namespace CJG.Web.External.Models.Shared
{
	public class BaseViewModel : BaseViewModel<int>
	{
	}

	public class BaseViewModel<T>
	{
		public T Id { get; set; }
		public string ReturnURL { get; set; }
		public string RedirectURL { get; set; }
		public List<KeyValuePair<string, string>> ValidationErrors { get; set; }
		public bool HasError => ValidationErrors != null && ValidationErrors.Any();

		/// <summary>
		/// Add the error to the model validation errors.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="error"></param>
		public void AddError(string key, string error)
		{
			if (ValidationErrors == null)
				ValidationErrors = new List<KeyValuePair<string, string>>();

			ValidationErrors.Add(new KeyValuePair<string, string>(key, error));
		}

		/// <summary>
		/// Add all the errors to the model validation errors.
		/// </summary>
		/// <param name="errors"></param>
		public void AddErrors(IEnumerable<KeyValuePair<string, string>> errors)
		{
			if (ValidationErrors == null)
				ValidationErrors = errors.ToList();
			else
				ValidationErrors.AddRange(errors);
		}

		public void AddConcurrencyError()
		{
			AddError("Summary", "Someone has updated the record. You must reload your page.");
		}
	}
}