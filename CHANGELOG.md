v2.0
- Modify Inventory to call new V3 API endpoints to retrieve certificates rather than scraping the managed website itself.  This will solve for 
  - The issue of the managed website being down and causing Inventory to fail to retrieve certificates.
  - The issue of mTLS certificates not being retrieved
- Add ability to inventory and manage both RSA and ECC certificates
- Add ability for 2 certificates (one RSA and one ECC) to be associated with a single domain name
- As a result of the above, certificate alias HAS TO be made up of the domain name, a pipe character delimiter (|), and the certificate type (RSA or ECC).  For example, a certificate alias for an RSA certificate for the domain name "example.com" would be "example.com|RSA".  A certificate alias for an ECC certificate for the same domain name would be "example.com|ECC".

v1.2.0
- Add .net10 build and remove .net6 

v1.1.2
- Modify error message to indicate comma separated API ID/API Key in the Store Password rather than pipe delimted

v1.1.1
- Bug fix to prevent API key logging 

v1.1.0
- Modified build to produce .net6 and .net8 compatible versions
- Updated README documentation to use doctool

v1.0.1
- Bug fix: Individual site timeouts will no longer end inventory but will instead skip that site and move on.  Inventory in those cases will produce a warning that certificates could not be retrieved for one or more sites, but it will still return all retrieved certificates.

v1.0
- Initial Version
