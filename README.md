SharePoint-2010-SAML-Claims-Provider
====================================

Custom claims provider for SAML authentication type of SharePoint 2010

This is a simple custom claims provider for SP 2010. 
The user source and trusted login provider is https://github.com/vadimi/node-sts project.

Install process:
1)  Install project as feature into SP farm;
2)	Register provider by script in scripts folder. Ensure that you set a correct trust login provider name.

More information on msdn site:
http://msdn.microsoft.com/en-us/library/gg251994
