<!-- add integration specific information below -->
## Versioning

The version number of a the Imperva Orchestrator Extension can be verified by right clicking on the Imperva.dll file in the Extensions/Imperva installation folder, selecting Properties, and then clicking on the Details tab.
&nbsp;  
&nbsp; 
## Imperva Orchestrator Extension Installation
1. Create the Imperva certificate store type manually in Keyfactor Command by clicking on Settings (the gear icon on the top right) => Certificate Store Types => Add and then entering the settings described in the next section - Certificate Store Type Settings, OR by utilizing the CURL script found under the Certificate Store Type CURL Script folder in this repo. 
2. Stop the Keyfactor Universal Orchestrator Service for the orchestrator you plan to install this extension to run on.
3. In the Keyfactor Orchestrator installation folder (by convention usually C:\Program Files\Keyfactor\Keyfactor Orchestrator), find the "Extensions" folder. Underneath that, create a new folder named "Imperva" (you may choose to use a different name if you wish).
4. Download the latest version of the Imperva Orchestrator Extension from [GitHub](https://github.com/Keyfactor/imperva-orchestrator).  Click on the "Latest" release link on the right hand side of the main page and download the first zip file.
5. Copy the contents of the download installation zip file to the folder created in Step 3.
6. (Optional) If you decide to create the certificate store type with a short names different than the suggested value of "Imperva", edit the manifest.json file in the folder you created in step 3, and modify each "ShortName" in each "Certstores.{ShortName}.{Operation}" line with the ShortName you used to create the certificate store type in Keyfactor Command.  If you created it with the suggested value, this step can be skipped.
7. Start the Keyfactor Universal Orchestrator Service.
&nbsp;  
&nbsp;  
## Certificate Store Type Settings
Below are the values you need to enter if you choose to manually create the Imperva certificate store type in the Keyfactor Command UI (related to Step 1 of Imperva Orchestrator Extension Installation above).
*Basic Tab:*
- **Name** – Required. The display name you wish to use for the new Certificate Store Type.  Suggested value - Imperva
- **ShortName** - Required. Suggested value - Imperva.  If you choose to use a different value, please refer to Step 6 under Imperva Orchestrator Extension Installation above.
- **Custom Capability** - Unchecked
- **Supported Job Types** - Inventory, Add, and Remove, should all be checked.
- **Needs Server** - Unchecked
- **Blueprint Allowed** - Checked if you wish to make use of blueprinting.  Please refer to the Keyfactor Command Reference Guide for more details on this feature.
- **Uses PoserShell** - Unchecked
- **Requires Store Password** - Checked.
- **Supports Entry Password** - Unchecked.  

*Advanced Tab:*  
- **Store Path Type** - Freeform
- **Supports Custom Alias** - Required
- **Private Key Handling** - Required
- **PFX Password Style** - Default  

*Custom Fields Tab:*
None

*Entry Parameters:*
None
&nbsp;  
&nbsp;  
