using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CJG.Core.Entities
{
	public class PaymentRequestAccountCode : EntityBase
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey(nameof(PaymentRequestBatch)), Column(Order = 0)]
		public int PaymentRequestBatchId { get; set; }

		/// <summary>
		/// get/set - The payment request batch that contains this payment request.
		/// </summary>
		public virtual PaymentRequestBatch PaymentRequestBatch { get; set; }

		/// <summary>
		/// get/set - The foreign key to the claim.
		/// </summary>
		[ForeignKey(nameof(Claim)), Column(Order = 1)]
		public int ClaimId { get; set; }

		/// <summary>
		/// get/set - The foreign key to the claim.
		/// </summary>
		[ForeignKey(nameof(Claim)), Column(Order = 2)]
		public int ClaimVersion { get; set; }

		/// <summary>
		/// get/set - The claim that is associated with this payment request.
		/// </summary>
		public virtual Claim Claim { get; set; }

		/// <summary>
		/// get/set - The GL account client number.
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "The general ledger client number is required."), MaxLength(50)]
		public string GLClientNumber { get; set; }

		/// <summary>
		/// get/set - The GL RESP account.
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "The general ledger RESP account number is required."), MaxLength(20)]
		public string GLRESP { get; set; }

		/// <summary>
		/// get/set - The GL service line account.
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "The general ledger service line account number is required."), MaxLength(20)]
		public string GLServiceLine { get; set; }

		/// <summary>
		/// get/set - The GL STOB account.
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "The general ledger STOB account number is required."), MaxLength(20)]
		public string GLSTOB { get; set; }

		/// <summary>
		/// get/set - The GL project code account.
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "The general ledger project code is required."), MaxLength(20)]
		public string GLProjectCode { get; set; }

	}
}