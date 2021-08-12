using System;
using CJG.Core.Entities;
using CJG.Core.Interfaces;
using Microsoft.CSharp.RuntimeBinder;
using RazorEngine.Templating;

namespace CJG.Application.Services.Exceptions
{
	public class TemplateException : Exception
	{
		/// <summary>
		/// get/set - The display name.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// get/set - The property name of the template.
		/// </summary>
		public string PropertyName { get; set; }


		public TemplateException(string propertyName, string displayName, TemplateCompilationException innerException) : base("The document template failed to compile.", innerException)
		{
			PropertyName = propertyName;
			DisplayName = displayName;
		}

		public TemplateException(string propertyName, string displayName, TemplateParsingException innerException) : base("The document template failed to parse.", innerException)
		{
			PropertyName = propertyName;
			DisplayName = displayName;
		}

		public TemplateException(string propertyName, string displayName, RuntimeBinderException innerException) : base("The document template failed to compile.", innerException)
		{
			PropertyName = propertyName;
			DisplayName = displayName;
		}


		public TemplateException(DocumentTypes documentType, TemplateCompilationException innerException) : this(documentType.ToString(), documentType.GetDescription(), innerException)
		{
		}

		public TemplateException(DocumentTypes documentType, TemplateParsingException innerException) : this(documentType.ToString(), documentType.GetDescription(), innerException)
		{
		}

		public string GetErrorMessages()
		{
			if (InnerException is TemplateCompilationException)
				return Message + "<br/>" + ((TemplateCompilationException)InnerException).GetErrorMessages();

			return this.GetAllMessages();
		}
	}
}
