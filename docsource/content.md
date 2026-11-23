## Overview

The Imperva Orchestrator Extension allows for the management of SSL certificates bound to web sites managed by the Imperva cloud-based firewall.
- Site domains may have up to 2 certificates bound to it, one RSA and one ECC certificate.
- Alias name for the certificate MUST BE in the format of "domain name|certificate type" where the certificate type is either RSA or ECC.  For example, a certificate alias for an RSA certificate for the domain name "example.com" would be "example.com|RSA".  A certificate alias for an ECC certificate for the same domain name would be "example.com|ECC".

**NOTE: for existing certificate stores upgrading from v1.x to v2.x or greater, run an inventory job after installing to get the alias names updated to the new format.  The inventory job will update the alias names for existing certificates in the store to the new format.**

## Requirements

The Imperva Orchestrator Extension requires the creation of an Imperva account id, API id, and API key.  Please reference the [Imperva Documentation](https://docs.imperva.com/bundle/cloud-application-security/page/settings/api-keys.htm#:~:text=In%20the%20Cloud%20Security%20Console%20top%20menu%20bar%2C%20click%20Account,to%20create%20a%20new%20key) for more information on creating an API id and key for use with this integration. 

