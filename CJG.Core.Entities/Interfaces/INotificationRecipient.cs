namespace CJG.Core.Entities.Interfaces
{
	public interface INotificationRecipient
	{
		string FirstName { get; set; }
		string LastName { get; set; }
		string EmailAddress { get; set; }
	}
}