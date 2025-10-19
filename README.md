# AI TaskFlow

An AI-assisted task management system inspired by GitHub Issues, designed to streamline project planning by automatically generating structured task hierarchies from textual business requirements.

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Getting Started](#getting-started)
- [Available Scripts](#available-scripts)
- [Project Scope](#project-scope)
- [Project Structure](#project-structure)
- [Project Status](#project-status)
- [License](#license)

## Overview

AI TaskFlow simplifies project setup and reduces manual task creation through intelligent parsing and generation of Epics, Stories, and Tasks. The system enables users to input structured Markdown text representing project requirements, from which AI generates a task hierarchy following the structure: **Epic → Story → Task**.

### Key Benefits

- **Automated Task Breakdown**: Converts business requirements into structured, hierarchical tasks
- **Review & Approval Workflow**: Users can manually review, edit, and approve AI-generated tasks before adding them to the main task list
- **Unified Task Management**: Combines AI-generated and manually created tasks in a single interface
- **Visual Progress Tracking**: Track tasks via tree view or Kanban board interfaces

## Tech Stack

### Frontend
- **Angular 20** - Modern SPA framework
- **TypeScript 5** - Static typing and enhanced IDE support
- **Tailwind 4** - Utility-first CSS framework
- **PrimeNG 20** - Rich UI component library (tree view, Kanban, forms)
- **Auth0 Angular** - Authentication integration

### Backend
- **.NET Core 9** - Cross-platform backend framework
- **ASP.NET Core Minimal APIs** - Streamlined REST API development
- **C#** - Primary programming language
- **Entity Framework Core** - ORM for database access
- **PostgreSQL** - Relational database
- **Semantic Kernel** - AI model integration (OpenAI)

### DevOps & Infrastructure
- **Auth0** - Authentication and authorization
- **GitHub Actions** - CI/CD pipelines
- **Docker** - Containerization
- **Hostinger** - Application hosting

## Features

### Core Functionality

- **AI Task Generation**
  - Manual trigger via "Generate" button
  - Accepts structured Markdown input (5,000-50,000 characters)
  - Generates one Epic with up to 10 Stories and 100 Tasks
  - Collapsible tree preview before approval

- **Task Approval Flow**
  - Draft mode for generated tasks
  - Approve all or select specific items via checkboxes
  - Transfer approved tasks to main project list

- **Task Management**
  - Manual CRUD operations for tasks
  - Fixed hierarchy: Epic → Story → Task
  - Cascade deletion (removes all child tasks)
  - Flexible status transitions

- **Progress Tracking**
  - Numeric ratio display (e.g., 2/10)
  - Visual progress bars for parent tasks
  - Manual refresh capability

- **User Management**
  - Secure authentication via Auth0
  - Task assignment to users
  - Role-based access (MVP: identical permissions)

## Getting Started

### Prerequisites

- **Node.js**: v22.19.0 (specified in `.nvmrc`)
- **.NET SDK**: v9.0.305
- **PostgreSQL**: Latest stable version
- **Auth0 Account**: For authentication setup

### Installation

#### Frontend Setup

```bash
cd src/frontend
npm install
npm start
```

The Angular application will run on `http://localhost:4200`

#### Backend Setup

```bash
cd src/backend
dotnet restore
dotnet build
dotnet run --project TaskFlow.Server
```

### Configuration

Create environment configuration files with the following settings:

**Frontend (`src/frontend/src/environments/environment.ts`)**:
```typescript
export const environment = {
  production: false,
  auth0Domain: 'YOUR_AUTH0_DOMAIN',
  auth0ClientId: 'YOUR_AUTH0_CLIENT_ID',
  apiUrl: 'http://localhost:5000'
};
```

**Backend (`src/backend/TaskFlow.Server/appsettings.Development.json`)**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_POSTGRESQL_CONNECTION_STRING"
  },
  "Auth0": {
    "Domain": "YOUR_AUTH0_DOMAIN",
    "Audience": "YOUR_AUTH0_AUDIENCE"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```

## Available Scripts

### Frontend Scripts

| Command | Description |
|---------|-------------|
| `npm start` | Start development server (`ng serve`) |
| `npm run build` | Build for production (`ng build`) |
| `npm run watch` | Build with watch mode (`ng build --watch --configuration development`) |
| `npm test` | Run unit tests (`ng test`) |

### Backend Scripts

| Command | Description |
|---------|-------------|
| `dotnet build` | Build the solution |
| `dotnet run --project TaskFlow.Server` | Run the API server |
| `dotnet test` | Run unit tests |

## Project Scope

### In Scope (MVP)

- AI-assisted task generation from structured Markdown
- Manual task creation, editing, and deletion
- Task hierarchy visualization (tree and Kanban views)
- User authentication and basic task assignment
- Manual approval and regeneration of AI-generated tasks
- Progress tracking with numeric indicators and progress bars
- Manual refresh for synchronization

### Out of Scope (MVP)

- Integration with external systems (GitHub Issues, Jira)
- Import of non-text formats (PDF, DOCX)
- Cross-project relationships between tasks
- Mobile applications
- Analytics and success metric dashboards
- Detailed audit logging

## Project Structure

```
10xDevs/
├── docs/
│   ├── prd.md              # Product Requirements Document
│   └── tech-stack.md       # Technology Stack Documentation
├── src/
│   ├── backend/
│   │   ├── modules/
│   │   │   ├── TaskFlow.Modules.Assistant/    # AI integration module
│   │   │   ├── TaskFlow.Modules.Users/        # User management module
│   │   │   └── TaskFlow.Modules.WorkItems/    # Task management module
│   │   ├── TaskFlow.Server/                   # Main API server
│   │   ├── Directory.Packages.props           # Central package management
│   │   ├── global.json                        # .NET SDK version
│   │   └── TaskFlow.sln                       # Solution file
│   └── frontend/
│       ├── src/                               # Angular application source
│       ├── .nvmrc                             # Node version specification
│       └── package.json                       # NPM dependencies
└── README.md
```

## Project Status

**Current Status**: MVP Development

The project is in active development phase, focusing on core functionalities:
- AI generation and approval workflow
- Manual task management
- Task tracking and visualization
- User authentication and assignment

This is a web-only application targeting modern browsers.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

For detailed product requirements and feature specifications, see [docs/prd.md](docs/prd.md).