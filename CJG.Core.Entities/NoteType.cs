using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CJG.Core.Entities
{
	/// <summary>
	/// <typeparamref name="NoteType"/> class, provides the ORM a way to manage note types.
	/// </summary>
	public class NoteType : LookupTable<NoteTypes>
	{
		/// <summary>
		/// get/set - The description for this note type.
		/// </summary>
		[MaxLength(500)]
		public string Description { get; set; }

		[DefaultValue(false)]
		public bool IsSystem { get; set; }

		/// <summary>
		/// Creates a new instance of a <typeparamref name="NoteType"/> object.
		/// </summary>
		public NoteType()
		{

		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="NoteType"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="description"></param>
		/// <param name="rowSequence"></param>
		public NoteType(NoteTypes type, string description, int rowSequence = 0) : base(type.GetDescription(), rowSequence)
		{
			Description = description;
		}

		/// <summary>
		/// Creates a new instance of a <typeparamref name="NoteType"/> object and initializes it with the specified property values.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="description"></param>
		/// <param name="isSystem">Is a system generated note.</param>
		/// <param name="rowSequence"></param>
		public NoteType(NoteTypes type, string description, bool isSystem, int rowSequence = 0) : this(type, description, rowSequence)
		{
			IsSystem = isSystem;
		}

		/// <summary>
		/// Returns a string in the following format - $"{Caption}"
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Caption;
		}
	}

	/// <summary>
	/// <typeparamref name="NoteTypes"/> enum, provides a list of all valid Note Type Id's.
	/// </summary>
	public enum NoteTypes
	{
		/// <summary>
		/// S25 - Note to Section 25
		/// </summary>
		S25 = 8,
		/// <summary>
		/// NE - Note to Evaluation
		/// </summary>
		NE = 19,
		/// <summary>
		/// MN - Note to Communication (Marketing & News Releases)
		/// </summary>
		MN = 21,
		/// <summary>
		/// PR - Note to Program Manager
		/// </summary>
		PR = 12,
		/// <summary>
		/// PD = Note to Director
		/// </summary>
		PD = 7,
		/// <summary>
		/// PM - Note to Policy
		/// </summary>
		PM = 13,
		/// <summary>
		/// QA - Note to QA
		/// </summary>
		QA = 20,
		/// <summary>
		/// NR - Note to Reimbursement
		/// </summary>
		NR = 14,
		/// <summary>
		/// ED - File Change
		/// </summary>
		ED = 15,
		/// <summary>
		/// NT - Notification
		/// </summary>
		NT = 16,
		/// <summary>
		/// WF - Workflow
		/// </summary>
		WF = 11,
		/// <summary>
		/// AA - Assessor Assigned
		/// </summary>
		AA = 17,
		/// <summary>
		/// TD - Training Date Change
		/// </summary>
		TD = 18
	}
}
