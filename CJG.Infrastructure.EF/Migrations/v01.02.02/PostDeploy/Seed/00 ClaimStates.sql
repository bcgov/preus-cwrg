PRINT 'Inserting [ClaimStates]'
SET IDENTITY_INSERT [dbo].[ClaimStates] ON 
INSERT [dbo].[ClaimStates]
 ([Id], [Caption], [Description], [IsActive], [RowSequence]) VALUES
 ( 0, N'Incomplete', N'A newly started Claim which in incomplete.', 1, 0)
,( 1, N'Complete', N'A Claim that is ready to be submitted.', 1, 0)
,( 21, N'NewClaim', N'A claim is submitted and claim assessment is the next action for assessors. Assessors assess and approve claims without director approval. The external state is "Claim Submitted"', 1, 0)
,( 22, N'Claim Under Assessment', N'An Assessor has selected a claim for assessmennt and is assigned to the grant file as the assessor.  The external state is "Claim Submitted"', 1, 0)
,( 23, N'Claim Returned To Applicant', N'The Claim needs more information and has been returned to the application administrator to edit and resubmit. The need for more information may include attached documentation required to substantiate the claim. The external state is "Claim Returned"', 1, 0)
,( 24, N'Claim Denied', N'The Claim has been denied and has a zero dollar assessment outcome and will show externally in the claim assessment block with the outcome of the assessment. The external state is "Claim Denied"', 1, 0)
,( 25, N'Claim Approved', N'The Claim has been approved and will now show externally in the claim assessment block with the outcome of the assessment. The external state is "Claim Approved"', 1, 0)
,( 26, N'Payment Requested', N'NOTE: This is not an grant file state and needs to be recorded somewhere as a child claim state PaymentRequested - The Claim has a payment request generated (printed) for it and the payment request was sent to the government''s payment department to have a cheque printed and sent to the application administrator.', 1, 0)
,( 27, N'Amount Owing', N'NOTE: This is not an grant file state and needs to be recorded somewhere as a child claim state AmountOwing - The assessment of a claim (version 2 or more) resulted in an amount owing from the applicant because the assessment of claim version 1 reimbursed too much money.  This state begins the collection process for the amount owing.', 1, 0)
,( 28, N'Claim Paid', N'NOTE: This is not an grant file state and needs to be recorded somewhere as a child claim state ClaimPaid - The CJG syhstem payment request for this claim has been reconciled against the payment transactions in the government''s accounts payable system and the payment request has be confirmed as paid.', 1, 0)
,( 29, N'Amount Received', N'NOTE: This is not an grant file state and needs to be recorded somewhere as a child claim state AmountReceived - The amount owing for a claim has been received from the applicant to the ministry and is confirmed in the government''s accounts receivable system as paid.  No amount is owing any more.', 1, 0)
SET IDENTITY_INSERT [dbo].[ClaimStates] OFF