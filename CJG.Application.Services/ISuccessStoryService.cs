using CJG.Core.Entities;
using CJG.Core.Interfaces.Service;

namespace CJG.Application.Services
{
	public interface ISuccessStoryService : IService
	{
		SuccessStory GetSuccessStory(int grantApplicationId);
		void AddSuccessStory(SuccessStory successStory);
		void UpdateSuccessStory(SuccessStory successStory);
		void ToggleSuccessStory(GrantApplication grantApplication);
	}
}