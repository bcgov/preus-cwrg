PRINT 'Inserting [ApplicationUsers]'
INSERT [dbo].[ApplicationUsers]
 ([InternalUserId], [ApplicationUserId], [Email], [SecurityStamp], [UserName], [Active]) VALUES

-- Internal UAT Users
 (2, N'3cfc3f41-93c2-4dda-ae0d-bbf7177ff6e8', N'laura.elliott@gov.bc.ca',				N'10bf0c21-f6c6-4fc0-9b9c-75be840f3221', N'lelliott', 1)
,(3, N'd42359d1-b297-46b8-946c-30e52df8c575', N'maria.chen@gov.bc.ca',					N'f3a2ced3-d848-418e-a4f1-749c21c3e3da', N'machen',   1)
,(4, N'72235ded-3045-4a00-8e08-bfdb47957617', N'nadaya.howells@gov.bc.ca',				N'b16f0c3d-0c9d-4b38-9f1a-90d318568959', N'nhowells', 1)
,(5, N'51c90c85-3a04-4f6c-84d6-a23701d089c5', N'lisa.tabachuk@gov.bc.ca',				N'92d3a6c7-666b-45a2-b51d-2d849a8f30fb', N'ltabachu', 1)
,(6, N'b7cefa1a-e0fa-4155-a8f6-a6331252740c', N'courtenay.wilson@gov.bc.ca',			N'fc7fb1d2-8245-416a-aac1-b2d60f126a04', N'couwilso', 1)
,(7, N'ce7e601e-80b2-4436-a46f-52e3711c1987', N'genevieve.casault@gov.bc.ca',			N'207f1ab6-a877-45bb-814a-1747ca80388b', N'gcasault', 1)
,(8, N'74edd439-defc-463d-96b1-3e5b797217df', N'robyn.wood@gov.bc.ca',					N'7279f79e-d017-41e8-a0c4-b81aa7a0f937', N'rmwood',   1)
,(9, N'f5d167fc-119e-4734-81b5-e3e85358db0d', N'chris.clarke@gov.bc.ca',				N'c1f5270f-1d64-46d5-859e-a41211f847f1', N'cclarke',  1)
,(10, N'19086edf-737c-4966-b7f2-d4ed4970d37b', N'nicoleta.turtureanu@gov.bc.ca',		N'a7a86103-1f86-43e7-8eb1-73ffb799d6be', N'nturture', 1)
,(11, N'ead65bc3-86ca-4f35-a83b-e6fad9b42566', N'erhan.baydar@gov.bc.ca',				N'84abd78a-9570-4f2d-8161-4a08a5614762', N'ebaydar',  1)
,(12, N'2c103774-39a3-49e5-a1de-a0540a5b7f70', N'michelle.beaubien@gov.bc.ca',			N'0b97c99f-0281-4fb4-b93f-ceb841dec576', N'mbeaubie', 1)

-- Internal Team Users
,(51, N'c52deb65-50a7-4cca-bbe9-902df4209a2c', N'tim.gerhardt@gov.bc.ca',				N'a63d197d-327a-4cc6-b35e-6204c12244bc', N'tigerhar', 1)
,(52, N'a2ede42f-e4df-48a7-8e06-acdc174a60af', N'matthew.mason@fcvinteractive.com',		N'2452f4a3-5ee9-439e-90cc-d45909936b39', N'mamason',  1)
,(53, N'54159c2b-8be3-4d70-a4cb-3d7ab53a5ef9', N'vance.mccoll@avocette.com',			N'bc8717f4-37d4-4cef-8ccc-b1928f8e9db0', N'vanmccol', 1)
,(54, N'5cc22b21-d0a2-498c-a78d-b9e1bcc91967', N'jeremy.foster@avocette.com',			N'6aead52d-287d-425b-b155-2e48c829e220', N'jefoster', 1)
,(55, N'78fdd78b-e018-4a72-8906-b49286065119', N'adam.lamping@fcvinteractive.com',		N'608e8eb3-8369-4878-8a85-842f3b9debfc', N'alamping', 1)
,(56, N'43a4dcd7-f69b-407b-8667-1a681c24d570', N'raman.samra@avocette.com',				N'd6c6f8e5-ec05-439f-b2a8-722e6b8e1367', N'rssamra',  1)
,(57, N'08741393-98b7-4e95-b73e-d56d8c7a7b27', N'shelly.saunders@avocette.com',			N'9a39b194-36ed-4fbc-9255-f4d6a88d4779', N'shsaunde', 1)
,(58, N'6756f909-b8a2-40ca-835f-6115df7d001f', N'TU1@avocette.com',						N'b3cb397e-f4bb-4a15-b1d0-3bde03cd5a1c', N'CJFTest1', 1)
,(59, N'7017e627-1e3a-4cc5-b730-ba7b7171cef3', N'TU2@avocette.com',						N'73585964-a39c-4b7c-a0ae-f6a3737823d4', N'CJFTest2', 1)
,(60, N'c729532a-b9d9-4175-a386-ed19cc94bf92', N'TU3@avocette.com',						N'720972aa-8808-45c0-9202-692c17881d34', N'CJFTest3', 1)	
,(61, N'482019c5-ddf9-46c7-aa7b-9ee760fbff82', N'TU4@avocette.com',						N'4f2a86fb-abcc-472f-8b18-4b37f7c0a93b', N'CJFTest4', 1)
,(62, N'2c3c7700-05d0-43de-93bc-440baf261d81', N'TU5@avocette.com',						N'38aea9c2-25fa-4f42-8fb9-2fc5a6977912', N'CJFTest5', 1)
,(63, N'8dbb0c49-cd49-48fa-9512-c80e4d581dfd', N'dave.penfold@fcvinteractive.com',		N'8ed599d7-8ce3-4ec9-a8c7-a1c2afc3c309', N'dpenfold', 1)
,(64, N'4ebb6bdd-228c-42fb-ace8-71cf72501b18', N'ryota.matsumoto@fcvinteractive.com',	N'2ecd0c43-5edc-447c-9cd0-bc912d27daca', N'rmatsumo', 1)