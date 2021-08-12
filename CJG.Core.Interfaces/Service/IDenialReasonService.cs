using CJG.Core.Entities;

namespace CJG.Core.Interfaces.Service
{
	public interface IDenialReasonService : IService
	{
		DenialReason Get(bool? isActive);
		DenialReason Get(int id);
		void Add(DenialReason denialReason);
	}
}
