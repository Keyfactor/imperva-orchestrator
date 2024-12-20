v1.1.1
- Modified build to produce .net6 and .net8 compatible versions
- Updated README documentation to use doctool

v1.0.1
- Bug fix: Individual site timeouts will no longer end inventory but will instead skip that site and move on.  Inventory in those cases will produce a warning that certificates could not be retrieved for one or more sites, but it will still return all retrieved certificates.

v1.0
- Initial Version
