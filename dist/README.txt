================================================================================
                            FAMILYFINANCES v0.6.7
                         Windows Standalone Edition
================================================================================

WELCOME

Thank you for using FamilyFinances! This is a portable, self-contained version
that runs entirely on your computer. No installation or .NET runtime needed.

================================================================================
QUICK START
================================================================================

1. STARTING THE APPLICATION

   Double-click:  Start FamilyFinances.bat
   
   This will:
   - Create necessary folders (data, logs)
   - Start the API and Web servers
   - Open your web browser automatically
   
   The application will be available at: http://localhost:5019

2. USING THE APPLICATION

   - Create your account on first use
   - Your data is stored locally in the 'data' folder
   - The app runs in your browser but all data stays on your computer

3. STOPPING THE APPLICATION

   Double-click:  Stop FamilyFinances.bat
   
   This will cleanly shut down both services.

================================================================================
FILE STRUCTURE
================================================================================

FamilyFinances-v0.6.7-win-x64/
  │
  ├── Start FamilyFinances.bat    ← Start the application
  ├── Stop FamilyFinances.bat     ← Stop the application
  ├── README.txt                  ← This file
  │
  ├── data/                       ← Your database (SQLite)
  │   └── familyfinances.db       (created on first run)
  │
  ├── logs/                       ← Application logs
  │   ├── api-YYYYMMDD.log        (daily log files)
  │   └── ...
  │
  ├── api/                        ← API server files
  │   └── FamilyFinances.Api.exe
  │
  └── web/                        ← Web server files
      └── FamilyFinances.Web.exe

================================================================================
TROUBLESHOOTING
================================================================================

PROBLEM: "Application won't start"

  1. Check if ports 5084 and 5019 are available
     - Close any programs using these ports
     - Or restart your computer
  
  2. Check the logs in the 'logs' folder
     - Look for error messages in api-YYYYMMDD.log
  
  3. Make sure Windows Firewall isn't blocking the application
     - You may see a firewall prompt on first run - click "Allow"

PROBLEM: "Browser doesn't open automatically"

  Manually open your browser and go to: http://localhost:5019

PROBLEM: "Can't access the application"

  1. Make sure both Start script windows are running (they will be minimized)
  2. Look for "FamilyFinances API" and "FamilyFinances Web" in Task Manager
  3. Check logs for error messages

PROBLEM: "Lost my data"

  Your database is in the 'data' folder. As long as this folder exists,
  your data is safe. To back up:
  
  1. Stop the application (Stop FamilyFinances.bat)
  2. Copy the entire 'data' folder to a safe location
  3. Restart the application

================================================================================
PORTS USED
================================================================================

  API:  http://localhost:5084
  Web:  http://localhost:5019

If these ports conflict with other software, you'll need to stop the other
application first.

================================================================================
SECURITY NOTES
================================================================================

  - This application runs locally on your computer only
  - It is NOT accessible from other computers on your network
  - Your data stays in the 'data' folder and is never sent anywhere
  - Logs contain system information but no sensitive financial data

================================================================================
SUPPORT
================================================================================

For issues, questions, or feature requests:
  GitHub: https://github.com/carlos-aisa/FamilyFinances

================================================================================
LICENSE
================================================================================

This software is provided as-is. See the LICENSE file in the source repository
for full terms.

================================================================================

Enjoy managing your family finances!
