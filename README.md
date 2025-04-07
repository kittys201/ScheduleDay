# ScheduleDay

ScheduleDay is a full-stack task management application built with Blazor WebAssembly and ASP.NET Core. It helps users organize daily activities, track progress, and improve productivity through an intuitive interface.

## 🚀 Features

- **User Authentication**: Secure login and registration with JWT
- **Task Management**: Create, edit, view, and delete tasks
- **Dashboard**: View all tasks with filtering and sorting options
- **Task Status Tracking**: Monitor task progress (Pending, In Progress, Completed)
- **Performance Metrics**: Track daily, weekly, and monthly task completion rates
- **Session Management**: Automatic session refresh and security timeout
- **Responsive Design**: Works on desktop and mobile devices

## 🛠️ Technology Stack

- **Frontend**: Blazor WebAssembly, Bootstrap 5
- **Backend**: ASP.NET Core 8.0
- **Authentication**: JWT (JSON Web Tokens)
- **Database**: PostgreSQL
- **Deployment**: Azure Web App
- **CI/CD**: Azure Pipelines

## 📋 Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL database
- Visual Studio 2022 or Visual Studio Code

### Installation

1. Clone the repository
   ```
   git clone https://github.com/kittys201/ScheduleDay.git
   ```

2. Navigate to the project directory
   ```
   cd ScheduleDay
   ```

3. Configure the database connection in `appsettings.json`
   ```json
   "ConnectionStrings": {
     "PostgresConnection": "YOUR_CONNECTION_STRING"
   }
   ```

4. Update JWT settings in `appsettings.json`
   ```json
   "JwtSettings": {
     "SecretKey": "YOUR_SECRET_KEY_AT_LEAST_32_CHARS",
     "Issuer": "YOUR_ISSUER",
     "Audience": "YOUR_AUDIENCE",
     "ExpirationInMinutes": 60
   }
   ```

5. Run database migrations
   ```
   dotnet ef database update
   ```

6. Run the application
   ```
   dotnet run
   ```

## 🧪 Testing

To run tests:
```
dotnet test
```

## 📱 User Guide

### Account Creation
1. Navigate to the homepage
2. Click "Register" to create a new account
3. Fill in your name, email, and password
4. Click "Register" to create your account

### Logging In
1. Enter your email and password
2. Optionally check "Keep me signed in"
3. Click "Login"

### Managing Tasks
- **Create Task**: Click "New Task" and fill in the details
- **View Tasks**: Select a task and click the eye icon
- **Edit Tasks**: Select a task and click the pencil icon
- **Delete Tasks**: Select a task and click the trash icon
- **Filter & Search**: Use the filter dropdown and search bar
- **Sort**: Toggle between ascending and descending date order

## 🔒 Security Features

- Password hashing with BCrypt
- JWT authentication with token expiration
- HTTPS enforcement
- CORS protection
- Session timeout after inactivity

## 🚢 Deployment

This application is deployed using Azure App Services. The server side and client side were deployed separately. You may access the web application at this URL: https://myscheduledayclient.azurewebsites.net/

Video demonstration (YouTube): https://www.youtube.com/video1

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Contributors

- Agustin Aguilar 
- Fernando Gonzalez 
- Jennifer Cristina Gonzalez 