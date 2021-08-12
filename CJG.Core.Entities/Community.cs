namespace CJG.Core.Entities
{
	/// <summary>
	/// A Community class, provides a way to manage a list of communities.
	/// </summary>
	public class Community : LookupTable<int>
    {
	    /// <summary>
        /// Creates a new instance of a Community object.
        /// </summary>
        public Community()
        {

        }
      
        /// <summary>
        /// Creates a new instance of a Community object and initializes it.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="rowSequence"></param>
        public Community(string caption, int rowSequence = 0) : base(caption, rowSequence)
        {

        }

		/// <summary>
		/// Get the Community name without the joined region (Relies on "Community - Region" naming)
		/// </summary>
		/// <returns></returns>
		public string GetCommunityName()
		{
			if (string.IsNullOrWhiteSpace(Caption))
				return string.Empty;

	        // Get the last index of a hyphen, an en-dash, or an em-dash
	        var indexOfDash = Caption.LastIndexOfAny(new [] { '-', '–', '—'} );
	        if (indexOfDash <= 0)
		        return Caption;

	        var communityName = Caption.Substring(0, indexOfDash)
		        .Trim('-', ' ')
		        .Trim('–', ' ')
		        .Trim('—', ' ');

	        return communityName;
        }

		/// <summary>
		/// Get the Region name without the Community Name (Relies on "Community - Region" naming)
		/// </summary>
		/// <returns></returns>
        public string GetRegionName()
		{
			if (string.IsNullOrWhiteSpace(Caption))
				return string.Empty;

			var indexOfDash = Caption.LastIndexOfAny(new[] { '-', '–', '—' });
			if (indexOfDash <= 0)
				return Caption;

			var regionName = Caption.Substring(indexOfDash)
		        .Trim('-', ' ')
		        .Trim('–', ' ')
		        .Trim('—', ' ');

			return regionName;
        }
	}
}
