namespace CJG.Core.Entities
{
	public enum ParticipantFundingStreams
	{
		// When we need to change the name to 'Other'
		ComprehensiveTariffs = 3,

		// When we only care about N/A
		NotApplicable = 8
	}

	public class ParticipantFundingStream : LookupTable<int>
	{
		public ParticipantFundingStream()
		{
		}

		public ParticipantFundingStream(string caption, int rowSequence = 0) : base(caption, rowSequence)
		{
		}
	}
}