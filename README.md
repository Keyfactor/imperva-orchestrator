# cpr-orchestrator-template

## Template for new Orchestrator projects

Use this repository to create new integrations for orcehstrator types. Update the following properties in the integration-manifest.json

* "name": "Integration Template",
* "status": "prototype",
* "description": "This project is meant to be a template to quickly build a basic integration product build. Currently in dev, a work in progress,",

For each platform, define which capabilities are present for this integration. You must update the boolean properties for both win and linux platforms.

* "supportsCreateStore"
* "supportsDiscovery"
* "supportsManagementAdd"
* "supportsManagementRemove"
* "supportsReenrollment"
* "supportsInventory"


If the repository is ready to be published in the public catalog, the following property must be updated:
* "update_catalog": true

When the repository is ready to be made public, the catalog must include a property to display the link to the github repo.
* "link_github": true