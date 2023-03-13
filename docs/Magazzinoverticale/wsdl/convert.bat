
rem "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" anagrafica.xsd /classes
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\svcutil.exe" anagrafica.xsd /t:code /l:c# /o:"anag.cs" /n:*,NamespaceName 
pause