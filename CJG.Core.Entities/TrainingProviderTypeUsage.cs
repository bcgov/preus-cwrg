namespace CJG.Core.Entities
{
	public enum TrainingProviderTypeUsage
	{
		/// <summary>
		/// Use for both Training Providers, and Employment Support Services
		/// </summary>
		All = 0,

		/// <summary>
		/// Use only for Training Providers
		/// </summary>
		TrainingProvider = 1,

		/// <summary>
		/// Use only for Employment Support Service Providers
		/// </summary>
		EmploymentSupportServices = 2
	}
}