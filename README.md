 SESH — Student Engagement & Support Hub

A digital system to enhance the Personal Supervisor process at the Department of Computer Science and Technology, enabling proactive student support through well-being tracking, meeting management, and data-driven insights.

Table of Contents

Overview

Features

Architecture

Installation

Usage

Testing

CI/CD Pipeline

Project Structure

Contributing

License

Overview

SESH transforms student support from a reactive model to a proactive, data-informed system.
It supports three main roles:

Students: Submit well-being reports & book meetings

Personal Supervisors: Monitor and support assigned students

Senior Tutors: View analytics and oversee engagement trends

Features

Students

Weekly well-being check-ins

Meeting booking with supervisors

Progress tracking & history

Secure role-based login

Personal Supervisors

Student dashboard with alerts

Automated “Struggling”/“In Crisis” notifications

Meeting and availability management

Student registration

Senior Tutors

Cohort-level analytics

Engagement tracking

Full user management

System oversight

Architecture

Tech Stack:

Backend: .NET 8.0 (C#)

Database: SQLite (EF Core)

Testing: xUnit, Moq

CI/CD: GitHub Actions

Design:

Repository & Service Layer patterns

Dependency Injection

Console-based MVC

Installation

Prerequisites:

.NET 8.0 SDK

Git

Visual Studio / VS Code

git clone https://github.com/YOUR-USERNAME/SESH.git
cd SESH
dotnet restore
dotnet build
dotnet run --project SESH


Default admin credentials:

Email: admin@hud.ac.uk

Password: admin123
