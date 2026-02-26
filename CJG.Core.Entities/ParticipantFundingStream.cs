namespace CJG.Core.Entities
{
	public enum ParticipantFundingStreams
	{
		// Only care about N/A
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