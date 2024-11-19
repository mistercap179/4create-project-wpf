# Employee Vacation Tracker Application

## Overview
The Employee Vacation Tracker is a desktop application designed to help managers and HR staff schedule and track employee vacation days. It ensures that each employee's vacation days are managed efficiently and that they cannot exceed their allocated vacation time.

## Solution Structure
The solution consists of two projects:

### 1. BusinessLogic (Class Library)
- Contains the business logic, including CRUD operations and communication with the database.
- The database operations are implemented using the **Code-First approach**, allowing the system to generate the database schema from the models.

### 2. WpfClient (WPF Application)
- This is the frontend application that allows users to interact with the system.
- It uses the **BusinessLogic** project to communicate with the database and manage vacation data.
- **Views and ViewModels for interaction (MVVM pattern)**.
- The WPF project integrates **Syncfusion UI components** for enhanced user interaction, such as calendars, combo boxes, and other advanced controls.

## Setup Instructions

### Prerequisites
- **Docker** (for running the database container)
- **.NET 8.0 SDK** (for running the application)

### Running the Application

#### 1. Clone the Repository
Clone the repository to your local machine using:

``` bash
git clone https://github.com/mistercap179/4create-project-wpf.git
```
### 2. Build and Run the Docker Database
Navigate to the root directory where the `docker-compose.yaml` file is located.  
Run the following commands to build and start the database container:

```bash
docker-compose down; docker-compose build; docker-compose up -d
```

### 3. Start the WPF Application
- Open the solution in **Visual Studio**.
- Ensure that all NuGet packages are installed or up-to-date:
- Right-click on the solution in **Solution Explorer** and select **Restore NuGet Packages**.
- Alternatively, you can open the **Package Manager Console** and run the following command to restore or update packages:
```bash
Update-Package -reinstall
```
-Build and run the application.


### 4. Logging
Logs for the application can be found in the following path:

```text
...\WpfClient\bin\Debug\net8.0-windows\logs\logfile.log
```

## Technologies Used
- **WPF** for the frontend user interface.
- **Entity Framework Core** for database operations using the **Code-First** approach.
- **Docker** for running the database in a containerized environment.
- **log4net** for logging important application events.
- **Syncfusion** for advanced UI components such as the **Calendar** and **ComboBox**.
