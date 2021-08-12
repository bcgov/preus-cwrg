using CJG.Core.Entities;
using CJG.Web.External.Models.Shared;

namespace CJG.Web.External.Areas.Ext.Models.ChangeRequest
{
	public class ApplicationAddressViewModel : AddressSharedViewModel
	{
		public ApplicationAddressViewModel() { }

		public ApplicationAddressViewModel(ApplicationAddress address) : base(address)
		{
		}
	}
}