using System.ComponentModel;

namespace CJG.Core.Entities
{
	public enum GrantWriterDesignation
	{
		[Description("Professional grant writer")]
		ProfessionalGrantWriter = 1,

		[Description("Non-professional grant writer")]
		NonProfessionalGrantWriter = 2,

		[Description("Training Service Provider (contractor)")]
		TrainingServiceProvider = 3
	}
}