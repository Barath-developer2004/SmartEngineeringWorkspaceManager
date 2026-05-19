# Smart Engineering Workspace Manager

A WPF (.NET Framework 4.8) application designed to help engineering teams manage projects, documents, tasks, and notifications efficiently in a modern workspace.

## Prerequisites
Before you start, make sure you have the following installed on your machine:
- **Windows OS** (required to run WPF applications)
- **.NET Framework 4.8 Developer Pack**
- **Visual Studio 2022** (Recommended, or Visual Studio 2019) with the **.NET desktop development workload** installed.

## How to Setup and Run
If you have cloned this repository and want to run it on your local machine, follow these simple steps:

1. **Clone the repository:**
   Open your preferred terminal / command line (e.g., Git Bash, Command Prompt) and run:
   ```bash
   git clone https://github.com/your-username/repository-name.git
   ```

2. **Open the Solution in Visual Studio:**
   - Double-click the `SmartEngineeringWorkspaceManager.sln` file located in the root folder, which will open the project in Visual Studio.

3. **Restore NuGet Packages:**
   - Visual Studio should automatically prompt you to restore NuGet packages when you open the solution. Allow it to finish.
   - Alternatively, you can right-click the Solution in the **Solution Explorer**, then select **"Restore NuGet Packages"**. This will ensure libraries like `System.Data.SQLite.Core` are downloaded.

4. **Build and Run:**
   - Go to the extremely top menu in Visual Studio and click **Build -> Build Solution** (or `Ctrl+Shift+B`).
   - Press **F5** or click the **"Start"** / **"Continue"** button (typically a green play arrow `▷`) at the top menu to run the application.

## Database Information
- The application uses **SQLite** as its local database, making it extremely lightweight and portable without requiring a standalone database server like SQL Server.
- Upon first running the app, it will automatically create a file called `WorkspaceData.sqlite` in the `.exe` output directory (`bin/Debug/` or `bin/Release/`).
- Default mock users (like an `admin` account) are instantiated upon DB creation to help you log in smoothly.

## Troubleshooting
- **Cannot find SQLite packages / build fails instantly:** Ensure NuGet packages were successfully restored. Check output logs for `Stub.System.Data.SQLite.Core.NetFramework`.
- **"CS0120: An object reference is required..." compilation error:** This has been resolved, make sure you pulled the latest commit!
