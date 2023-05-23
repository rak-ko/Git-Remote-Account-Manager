# Github Account Manager
## About
- Simple command line app for managing github accounts
- Automatically sets ssh config
- Windows only
- **!!! The ssh config file will be completely overwritten. Put any of your custom config files into a 'customConfigs' folder inside the default .ssh folder !!!**
- **!!! All ssh keys have to be inside the default .ssh folder !!!**

## Requirements
- Git
- ssh-keygen command
- Ssh version above 7.3p1 (If you want to use your own ssh config files)

## Installation
1) Download the 'gam.exe' file from releases
2) Put it inside a separate folder (Location doesn't matter)
3) Add that folder's path to the PATH variable
4) If you have any ssh config files move them to a file named 'customConfigs' inside the .ssh folder and they will be included inside the automatic config
5) You can now call 'gam' from anywhere

## Troubleshooting
- If you get any 'access to the path x is denied' try either moving the program somewhere, where you don't need administrator priviliges to run (Like documents or the desktop). Or run the terminal with administrator rights.
