using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IParticipantFundingStreamService : IService
	{
		ParticipantFundingStream Get(int id);
		IEnumerable<ParticipantFundingStream> GetAll();
		IEnumerable<ParticipantFundingStream> GetAll(bool isActive);

		bool IsInUse(int participantFundingStreamId);
		void UpdateFundingStreams(List<ParticipantFundingStream> streamList, List<ParticipantFundingStream> removeItems);
	}
}