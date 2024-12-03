namespace CJG.Core.Entities.Helpers
{
	public struct NotificationTypeFilter
	{
		public NotificationTriggerTypes NotificationTriggerId { get; }
		public string Caption{ get; }
		public string OrderBy { get; }

		public NotificationTypeFilter(NotificationTriggerTypes triggerId, string caption, string orderBy = null)
		{
			NotificationTriggerId = triggerId;
			Caption = caption;
			OrderBy = orderBy;
		}
	}
}
