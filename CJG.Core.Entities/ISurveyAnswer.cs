namespace CJG.Core.Entities
{
	public interface ISurveyAnswer
	{
		string OptionTextDisplayed { get; set; }
		bool Answer { get; set; }
		string TextAnswer { get; set; } // This holds either the 'Other' answer, the 'FreeText' answer, or the ReplacementToken Translated answer 
	}
}