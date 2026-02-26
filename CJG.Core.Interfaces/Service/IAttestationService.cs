namespace CJG.Core.Interfaces.Service
{
	public interface IAttestationService : IService
	{
		void DeleteExistingAttestationParticipant(int grantApplicationId);
	}
}