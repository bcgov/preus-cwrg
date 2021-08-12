namespace CJG.Core.Entities
{
	public class ApplicantOrganizationType : LookupTable<int>
    {
	    public const int OtherTypeId = 21;

	    /// <summary>
        /// Creates a new instance of a <typeparamref name="ApplicationType"/> object.
        /// </summary>
        public ApplicantOrganizationType() : base()
        {

        }

        /// <summary>
        /// Creates a new instance of a <typeparamref name="ApplicationType"/> object and initializes it with the specified property values.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="rowSequence"></param>
        public ApplicantOrganizationType(string caption, int rowSequence = 0) : base(caption, rowSequence)
        {

        }
    }
}
