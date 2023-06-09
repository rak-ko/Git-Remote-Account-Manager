# Git Remote Account Manager
## About
- Simple command line app for managing git remote accounts
- Automatically sets ssh config
- Windows/Linux
- **!!! All ssh keys have to be inside the default .ssh folder !!!**

## Requirements
- Git
- ssh-keygen command
- Ssh version above 7.3p1

## Installation
### Windows
1) Download the 'gam-windows-x64.exe' file from releases
2) Put it inside a separate folder (Location doesn't matter) and rename it to gam.exe
3) Add that folder's path to the PATH variable
4) You can now call 'gam' from anywhere
### Linux
1) Download the 'gam-linux-x64' file from releases
2) Rename it to gam and put it in a separate folder (Location doesn't matter)
3) Add that folder's path to the PATH variable
4) Run 'chmod +x gam' inside the folder
5) You can now call 'gam' from anywhere

## Troubleshooting
- If you get any 'access to the path x is denied' try either moving the program somewhere, where you don't need administrator privileges to run (Like documents or the desktop). Or run the terminal with administrator rights
- If it keeps getting flagged by your antivirus, try excluding the folder you put it in/the file itself
