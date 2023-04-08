# Blitz Troubleshooter

![Blitz.gg Logo](https://i.imgur.com/q7t8fzT.png)

This is a troubleshooter for [Blitz.gg](https://blitz.gg/). It is a utility designed to resolve common issues with the Blitz application.

## Features

- Fix all common issues
- Fix potential Overlay issues by running Blitz as Administrator
- Fix Blitz not starting up on boot
- Clear Cache
- Uninstall Blitz

## How it Works

The Blitz Troubleshooter is a C# program that uses various techniques to diagnose and fix issues related to the Blitz application. The codebase includes functionality for killing Blitz processes, removing cache files, setting startup options, and uninstalling the application. It also provides a user interface with different language options, allowing users to select their preferred language for better accessibility.

### Key Functions

1. **KillBlitz**: Terminates all running instances of the Blitz application.
2. **AppdataBlitz**: Clears the Blitz cache by removing Blitz-related folders from the user's AppData directory.
3. **Uninstall**: Uninstalls the Blitz application by deleting all associated directories.
4. **EnableBtn** and **DisableBtn**: Enable or disable buttons in the user interface to prevent unwanted actions during certain operations.
5. **Client_DownloadProgressChanged** and **Client_DownloadFileCompleted**: Handles download progress and completion events when downloading the Blitz application.
6. **SetLanguageDictionary**: Sets the language dictionary for the user interface based on the user's selection.
7. **BtnFixOverlayClick**: Sets both the League of Legends client and the Blitz client to run as administrator to fix the overlay issue.
8. **BtnRemoveAdminClick**: Removes the "Run as Administrator" setting for both the League of Legends client and the Blitz client.
9. **BtnFixBootClick**: Sets the Blitz client to start automatically on system boot.
10. **BtnRemoveBootFixClick**: Removes the Blitz client from the list of applications that start automatically on system boot.
11. **Button_Click_2**: Clears the Blitz cache.
12. **IsBlitzInstalled**: Checks if the Blitz application is installed on the system.
13. **Button_Click_4**: Uninstalls the Blitz application if it is installed.

The program also includes additional functions for handling UI events, such as changing the background color and updating the language selection.

To use the Blitz Troubleshooter, simply run the executable and follow the on-screen instructions.
