PRINT 'Starting Update of CWRG Completion Report Questions'

PRINT 'Inserting [Completion Report Question]'

INSERT INTO [CompletionReportQuestions] ([CompletionReportId], [Question], [Description], Audience, GroupId, [Sequence], IsRequired, IsActive, QuestionType, DefaultText, DateAdded, DisplayOnlyIfGoto)
VALUES (2, 
'What part of the application process was the most difficult and why?',
'Open-ended question for CWRG Employers', 
1, 9, 17, 1, 1, 3,
'Default text', 
GETUTCDATE(), 0)

INSERT INTO [CompletionReportQuestions] ([CompletionReportId], [Question], [Description], Audience, GroupId, [Sequence], IsRequired, IsActive, QuestionType, DefaultText, DateAdded, DisplayOnlyIfGoto)
VALUES (2, 
'What, if any, were the problems in recruiting?',
'Open-ended question for CWRG Employers', 
1, 9, 18, 1, 1, 3,
'Default text', 
GETUTCDATE(), 0)

INSERT INTO [CompletionReportQuestions] ([CompletionReportId], [Question], [Description], Audience, GroupId, [Sequence], IsRequired, IsActive, QuestionType, DefaultText, DateAdded, DisplayOnlyIfGoto)
VALUES (2, 
'Were the Participant Financial Supports sufficient to keep participants in training or to cover training costs?',
'Open-ended question for CWRG Employers', 
1, 9, 19, 1, 1, 3,
'Default text', 
GETUTCDATE(), 0)

INSERT INTO [CompletionReportQuestions] ([CompletionReportId], [Question], [Description], Audience, GroupId, [Sequence], IsRequired, IsActive, QuestionType, DefaultText, DateAdded, DisplayOnlyIfGoto)
VALUES (2, 
'Was the request for approving marketing materials fulfilled in a timely manner (how many days)?',
'Open-ended question for CWRG Employers', 
1, 9, 20, 1, 1, 3,
'Default text', 
GETUTCDATE(), 0)

INSERT INTO [CompletionReportQuestions] ([CompletionReportId], [Question], [Description], Audience, GroupId, [Sequence], IsRequired, IsActive, QuestionType, DefaultText, DateAdded, DisplayOnlyIfGoto)
VALUES (2, 
'Was administering the training a burden on your organization’s capacity and do you have suggestions for improvement? ',
'Open-ended question for CWRG Employers', 
1, 9, 21, 1, 1, 3,
'Default text', 
GETUTCDATE(), 0)

PRINT 'Done Update of CWRG Completion Report Questions'
