# 📋 Project & Task Management System

A web-based **Project & Task Management System** built using **ASP.NET MVC**, **Entity Framework**, **SQL Server**, **Razor Views**, **Bootstrap**, and **ASP.NET Identity**.

The system helps organizations manage projects, tasks, team members, project managers, and user roles from a centralized dashboard.

---

## 🚀 Project Overview

The Project & Task Management System provides a centralized platform where users can:

- Create and manage projects
- Assign project managers
- Add and remove project team members
- Create and assign tasks
- Track task progress
- Manage task priorities and statuses
- Monitor overdue tasks
- View project progress
- Manage users and roles
- Authenticate users securely
- Display role-based dashboards

The system is designed with **role-based access control**, so different users can access different features depending on their role.

---

## ✨ Main Features

### 🔐 Authentication

- User registration
- User login
- Logout
- Remember Me functionality
- Show/Hide password
- Password confirmation
- Anti-forgery token protection
- Role-based authentication

### 👥 User Roles

The application supports different roles such as:

- **Admin**
- **Project Manager**
- **Team Member**

Each role has different permissions and available features.

---

## 📊 Dashboard

The dashboard provides an overview of the system.

It displays:

- Total Projects
- Active Projects
- Total Tasks
- Tasks In Progress
- Completed Tasks
- Pending Tasks
- Overdue Tasks
- Recent Projects
- Assigned Tasks
- Project progress
- Current user's role

The dashboard is customized according to the authenticated user's role.

---

## 📁 Project Management

Administrators and authorized users can manage projects.

Project functionality includes:

- Create Project
- Edit Project
- View Project Details
- Assign Project Manager
- Set Department
- Set Project Status
- Set Start Date
- Set End Date
- Track Project Progress
- View project tasks
- View project team members

---

## 👨‍💼 Team Member Management

Project managers can manage project team members.

Features include:

- Add team members
- Remove team members
- View member name
- View member email
- View assigned date
- View team members associated with a project

---

## ✅ Task Management

The task management module allows users to create and track project tasks.

Task features include:

- Create tasks
- Assign tasks to users
- Set task priority
- Set task status
- Set due date
- Track task progress
- View task details
- Identify overdue tasks
- Associate tasks with projects

### Task Priorities

- Low
- Medium
- High
- Critical

### Task Statuses

- Pending
- In Progress
- Completed

---

## 🎨 User Interface

The application uses a modern responsive interface built with:

- Bootstrap
- Bootstrap Icons
- Custom CSS
- Razor Views
- Responsive layouts
- Cards
- Tables
- Progress bars
- Badges
- Responsive forms

The login and registration pages include modern UI designs with:

- Gradient backgrounds
- Project management branding
- Responsive layout
- Password visibility toggle
- Feature highlights
- Security information

---

# 🛠️ Technologies Used

### Backend

- C#
- ASP.NET MVC 5
- .NET Framework
- Entity Framework
- ASP.NET Identity

### Frontend

- HTML5
- CSS3
- Bootstrap
- Bootstrap Icons
- JavaScript
- Razor View Engine

### Database

- Microsoft SQL Server
- Entity Framework ORM

### Development Tools

- Microsoft Visual Studio
- SQL Server Management Studio
- GitHub

---

# 🏗️ Project Architecture

The project follows the MVC architecture.

```text
ProjectManagementSystem
│
├── Controllers
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── ProjectsController.cs
│   ├── TasksController.cs
│   └── UsersController.cs
│
├── Models
│   ├── ApplicationUser.cs
│   ├── Project.cs
│   ├── ProjectMember.cs
│   ├── TaskItem.cs
│   ├── Department.cs
│   └── Enums
│
├── Models/ViewModels
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── ProjectDetailsViewModel.cs
│   └── ProjectMemberViewModel.cs
│
├── Views
│   ├── Account
│   ├── Dashboard
│   ├── Projects
│   ├── Tasks
│   ├── Users
│   ├── Home
│   └── Shared
│
├── Content
│   └── CSS files
│
├── Scripts
│   └── JavaScript files
│
├── App_Start
│
├── Web.config
│
└── Global.asax
