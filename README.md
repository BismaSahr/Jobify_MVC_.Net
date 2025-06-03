# Jobify_MVC_.Net
Jobify is a full-featured, role-based job portal built with ASP.NET MVC 5 and Entity Framework. It supports three main user roles: Admin, Employer, and Job Seeker, each with tailored dashboards and functionality. The system allows job posting, job applications, and user management in a clean and responsive interface .


## 🔍 Features

### 👥 Role-Based Access
- **Admin**: Manage users (Admins, Employers, Job Seekers), view site-wide analytics.
- **Employer**: Post and manage job listings, view applications.
- **Job Seeker**: Browse jobs, apply for job,saved jobs.
  
### 📊 Interactive Dashboards
- **Chart.js** integration for visual summaries:
-Used pie charts

### 📝 Core Functionalities
- User registration and login with role assignment
- Full CRUD operations for:
  - Job Posts
  - Applications
  - User Profiles
  - Categories
  - Locations
- Saved jobs functionality
- Job Applications

## 🛠 Tech Stack

- **Frontend**: Razor Views, HTML5, CSS3, Bootstrap 4, Font Awesome
- **Backend**: ASP.NET MVC 5 (C#)
- **ORM**: Entity Framework 6
- **Database**: SQL Server
- **Charts**: [Chart.js](https://www.chartjs.org/)

## 📦 Project Structure
-/Controllers → MVC Controllers (UsersController, AdminsController, etc.)
-/Models → Entity Framework models
-/ViewModels → Dashboard-specific view models
-/Views → Razor views organized by controller
-/Scripts → JavaScript files and Chart.js logic
-/App_Data → SQL Server local DB

  

