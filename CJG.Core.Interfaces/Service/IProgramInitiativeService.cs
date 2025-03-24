using System.Collections.Generic;
using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IProgramInitiativeService : IService
	{
		ProgramInitiative Get(int id);
		IEnumerable<ProgramInitiative> GetAll();
		IEnumerable<ProgramInitiative> GetAll(bool isActive);

		bool IsInUse(int programInitiativeId);
		void UpdateInitiatives(List<ProgramInitiative> initiativeList, List<ProgramInitiative> removeItems);
	}
}