-- Some environments have an invalid Scheduled Task, breaking deployment of v29.7.0

DELETE
FROM CMS_ScheduledTask
WHERE TaskGUID NOT IN (
'F6A7D4DC-0146-4966-BE83-F7E3A87DF41C',
'7C93F262-FDAD-4571-A568-EB0AD882825D',
'741780C2-0FE0-40FF-9A62-D7E457DE46F4',
'BE17499A-7695-4FD4-B138-EE01F3BDD196',
'52EBFDA1-69C0-4101-8CCC-E200F86881B6',
'C2221537-ADC3-47EC-93D1-25CD1CCDBA00',
'8703D71F-E6EF-4312-AFCA-5BB8DA2DCFD5',
'010EEC94-BD46-40FC-9951-A0A190ED211C',
'59FDA4D8-F73C-4711-8E8F-A8E2F843108E',
'99C68980-6627-4E05-8008-FD4A4377E56A',
'D82BE54B-D55B-4AAE-BD3D-3EF859A312FB')