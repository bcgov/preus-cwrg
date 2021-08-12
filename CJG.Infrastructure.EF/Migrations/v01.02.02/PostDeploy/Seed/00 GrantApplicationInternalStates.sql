PRINT 'Inserting [GrantApplicationInternalStates]'
SET IDENTITY_INSERT [dbo].[GrantApplicationInternalStates] ON 
INSERT [dbo].[GrantApplicationInternalStates]
 ([Id], [Caption], [Description], [IsActive], [RowSequence]) VALUES
 ( 0, N'Draft', N'The Grant Application is being created by the Application Administrator but is not submitted.', 1, 0)
,( 1, N'New', N'The Grant Application has been submitted by the Application Administrator and is part of the intake for a grant opening.  It is waiting to be selected for assessment.  The external state is "Submitted"', 1, 0)
,( 2, N'Pending Assessment', N'The Grant Application has been selected for assessment with funds reserved to allow a successful assessment.  The grant file in this state has its application waiting in a queue to be assigned to an assessor for assessment.  The external state is "Submitted"', 1, 0)
,( 3, N'Under Assessment', N'The Grant Application is currently under assessment by the Assessor assigned to the grant file.  The external state is "Submitted"', 1, 0)
,( 4, N'Returned To Assessment', N'The Grant Application has been reviewed by the Director and returned to the Assessor for more information.  The external state is "Submitted"', 1, 0)
,( 5, N'Recommended For Approval', N'The assessor has recommended to the director that the grant application be approved.  The external state is "Submitted"', 1, 0)
,( 6, N'Recommended For Denial', N'The assessor has recommended to the director that the grant application be denied.  The external state is "Submitted"', 1, 0)
,( 7, N'Offer Issued', N'As the first step in approval for a grant, the Director has issued an offer in the form of an Agreement for acceptance by the application administrtor.  The agreement must be accepted by the application administrator for the grant to be approved and participant and claim reporting to begin.  The external state is "AcceptGrantAgreement"', 1, 0)
,( 8, N'Offer Withdrawn', N'The Agreement has been withdrawn by the Minitry before Application Administrator acceptance. This occurs at the discretion of   the director when the deadline for accepting the agreement passes.  The director controls when the offer is withdrawn and may choose to provide  more time for acceptance.  Notifications inform the application administrator of the need to accept the agreement by the deadline.  The external state is "AgreementWithdrawn"', 1, 0)
,( 9, N'Agreement Accepted', N'The Agreement has been accepted by the Application Administrator (who represents the Applicant business).  This ends successful application submission and assessment and, externally, the grant file moves on to (stage 2) enabling reporting for participants, claims and completion.  There is no internal activity required now until a change request or claim is reported.  The agreement may also be cancelled by either party.  The external state is "Approved" because the grant is approved; this is more relevant than showing "Agreement Accepted" externally which the user already knows.', 1, 0)
,( 10, N'Unfunded', N'The Grant Application did not receive funding and was returned to the application administrator in a state where it can be edited and resubmitted.  The external state is "Not Accepted" because the application was not accepted due to insufficient funding.', 1, 0)
,( 11, N'Application Denied', N'The Grant Application has been denied.  The external sate is "Application Denied"', 1, 0)
,( 12, N'Agreement Rejected', N'The Agreement was rejected by the Application Administrator.  The external state is "Closed" because the grant file is externally closed by this action.  Messages to the application administrator advise them of this outcome should then choose to reject the agreement.', 1, 0)
,( 13, N'Application Withdrawn', N'The Application was withdrawn by the Application Administrator before an Agreement was offered.  The external state is "Application Withdrawn"', 1, 0)
,( 14, N'Cancelled By Ministry', N'The Agreement was cancelled by the ministry after it was accepted by the Application Administrator.  The external state is "Cancelled by Ministry"', 1, 0)
,( 15, N'Cancelled By Agreement Holder', N'The Agreement was cancelled by the Application Administrator after accepting.  The external state is "Cancelled by Agreement Holder"', 1, 0)
,( 16, N'Change Request', N'The Application Administrator has made a change request on the accepted Grant Application and  the request is new for Ministry assessment.  The external state is "Change Request Submitted"', 1, 0)
,( 17, N'Change For Approval', N'The change request was recommended to the director for approval by the Assessor.  The external state is "Change Request Submitted" until the change request has been assessed.', 1, 0)
,( 18, N'Change For Denial', N'The change request was recommended to the director for denied by the director.  The external state is "Change Request Submitted" until the change request has been assessed.', 1, 0)
,( 19, N'Change Returned', N'The change request was returned to assessment by the director for more information.  The external state is "Change Request Submitted" until the change request has been assessed.', 1, 0)
,( 20, N'Change Request Denied', N'The change request was denied by the director.  The external state is "Change Request Denied" and allows another change request to be made.', 1, 0)
,( 21, N'New Claim', N'A claim is submitted and claim assessment is the next action for assessors.  Assessors assess and approve claims without director approval.  The external state is "Claim Submitted"', 1, 0)
,( 22, N'Claim Under Assessment', N'An Assessor has selected a claim for assessmennt and is assigned to the grant file as the assessor.  The external state is "Claim Submitted"', 1, 0)
,( 23, N'Claim Returned To Applicant', N'The Claim needs more information and has been returned to the application administrator to edit and resubmit.  The need for more information may include attached documentation required to substantiate the claim.  The external state is "Claim Returned"', 1, 0)
,( 24, N'Claim Denied', N'The Claim has been denied and has a zero dollar assessment outcome and will show externally in the  claim assessment block with the outcome of the assessment.  The external state is "Claim Denied"', 1, 0)
,( 25, N'Claim Approved', N'The Claim has been approved and will now show externally in the claim assessment block with the outcome of the assessment.  The external state is "Claim Approved"', 1, 0)
,( 30, N'Closed', N'The Grant Application is closed and can no longer be edited.', 1, 0)
SET IDENTITY_INSERT [dbo].[GrantApplicationInternalStates] OFF