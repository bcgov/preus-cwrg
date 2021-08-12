PRINT 'Updating CWRG Reimbursement Claim Notification Template'

-- Update Change Request Denied Template
UPDATE dbo.[NotificationTemplates]
SET [EmailBody] = 
'<!DOCTYPE html><html><head/><body>
@if(Model.IsPayment) {
<p>Dear @Model.RecipientFirstName,</p>
<p>Your reimbursement claim for Grant Agreement <strong>#@Model.FileNumber,</strong> "@Model.ProgramTitle," has been processed and a reimbursement in the amount of @Model.ReimbursementPayment will be sent to you by electronic funds transfer or by a cheque mailed to the head office mailing address submitted with your application.</p>
<p>If you have any questions, please contact us at <a href="mailto:@Model.ProgramEmail">@Model.ProgramEmail</a>.</p>
<p>Thank you</p>
<p>@Model.ProgramName Team</p>
} else {
<p>Dear @Model.RecipientFirstName,</p>
<p>Your reimbursement claim for Grant Agreement<strong> #@Model.FileNumber,</strong> "@Model.ProgramTitle," has been reviewed, and an overpayment in the amount of @Model.ReimbursementPayment has been determined. Please mail a cheque made payable to the "Minister of Finance", and send to the following address within 14 days:</p>
<p>Ministry of Advanced Education, Skills &amp; Training<br />Attention: Finance Unit<br />PO Box 9189 Stn Prov Govt<br />Victoria BC V8W 9E6</p>
<p>If you have any questions, please contact us at <a href="mailto:@Model.ProgramEmail">@Model.ProgramEmail</a>.</p>
<p>Thank you,</p>
<p>@Model.ProgramName Team</p>
}
</body></html>',
	DateUpdated = GETUTCDATE()
WHERE [Id] = 18

PRINT 'Done updating CWRG Reimbursement Claim Notification Template'

