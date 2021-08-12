using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public class DuplicateSINParticipants
	{
		public ParticipantForm PrimaryForm { get; set; }
		public IEnumerable<ParticipantForm> DuplicatedParticipantForms { get; set; }
	}
}