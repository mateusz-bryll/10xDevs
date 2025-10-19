# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AI TaskFlow is an AI-assisted task management system that generates structured task hierarchies (Epic → Story → Task) from Markdown-formatted business requirements. The system uses AI to automate project planning, with a review/approval workflow before tasks are committed to the main project board.

## Tech Stack

**Frontend:** Angular 20 + TypeScript 5 + Tailwind 4 + PrimeNG 20 + Auth0 Angular
**Backend:** .NET Core 9 + ASP.NET Core Minimal APIs + Entity Framework Core + PostgreSQL
**AI Integration:** Semantic Kernel (OpenAI)
**Hosting:** Docker + Hostinger
**CI/CD:** GitHub Actions

## Architecture

### Modular Backend Structure

The backend follows a **modular monolith architecture** with three distinct modules:

- **TaskFlow.Modules.Assistant** - AI integration for task generation using Semantic Kernel
- **TaskFlow.Modules.Users** - User authentication and management via Auth0
- **TaskFlow.Modules.WorkItems** - Core task management (CRUD, hierarchy, progress tracking)
- **TaskFlow.Server** - Main ASP.NET Core API host using Minimal APIs

All modules are referenced in `TaskFlow.sln` and managed via **Central Package Management** (`Directory.Packages.props`). When adding NuGet packages, update the centralized package versions in `Directory.Packages.props` rather than individual project files.

### Frontend Structure

Angular application uses standalone components (no NgModules). PrimeNG components provide tree view, Kanban board, and form controls aligned with PRD requirements.

## Development Commands

### Frontend (from `src/frontend/`)

```bash
npm install          # Install dependencies
npm start            # Start dev server (http://localhost:4200)
npm run build        # Production build
npm test             # Run Jasmine unit tests via Karma
npm run watch        # Build with watch mode
```

### Backend (from `src/backend/`)

```bash
dotnet restore                          # Restore NuGet packages
dotnet build                            # Build entire solution
dotnet run --project TaskFlow.Server    # Run API server
dotnet test                             # Run unit tests (when implemented)
```

**Note:** Backend currently contains minimal Program.cs scaffold. Modules need implementation for actual functionality.

### Version Requirements

- **Node.js:** v22.19.0 (specified in `src/frontend/.nvmrc`)
- **.NET SDK:** v9.0.305 (specified in `src/backend/global.json`)
- **PostgreSQL:** Latest stable version

## Key Design Patterns

### Task Hierarchy

Fixed three-level hierarchy enforced throughout the system:
- **Epic** (top level) → contains Stories
- **Story** (middle level) → contains Tasks
- **Task** (leaf level) → no children

**Cascade deletion:** Deleting a parent removes all children. Always confirm with user before executing.

### AI Generation Workflow

1. User inputs structured Markdown (5,000-50,000 characters)
2. AI generates **one Epic** with up to **10 Stories** and **100 Tasks**
3. Generated tasks stored as **drafts** (client-side only in MVP)
4. User reviews via collapsible tree preview
5. User approves all or selected tasks via checkboxes
6. Approved tasks transferred to main project list via dedicated endpoint
7. "Regenerate all" replaces entire draft (requires confirmation)

### Manual Refresh Model

**No live synchronization.** All views (tree, Kanban) require manual refresh via refresh button. Progress tracking updates on refresh only.

### Progress Calculation

- Parent tasks show progress as numeric ratio: `2/10` (2 of 10 children completed)
- Visual progress bar displayed alongside ratio
- Tasks without children show: `-/-` with grayed-out progress bar

## Configuration

### Frontend Environment

Create `src/frontend/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  auth0Domain: 'YOUR_AUTH0_DOMAIN',
  auth0ClientId: 'YOUR_AUTH0_CLIENT_ID',
  apiUrl: 'http://localhost:5000'
};
```

### Backend Settings

Update `src/backend/TaskFlow.Server/appsettings.Development.json`:

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

## Constraints and Scope

### MVP In Scope

- AI task generation from Markdown
- Manual task CRUD operations
- Tree view and Kanban board visualization
- User authentication (Auth0) and self-assignment
- Manual approval workflow for AI-generated tasks
- Progress tracking with manual refresh

### MVP Out of Scope

- External integrations (GitHub, Jira)
- Non-text file imports (PDF, DOCX)
- Cross-project task relationships
- Mobile applications
- Analytics dashboards
- Detailed audit logging
- Real-time synchronization

### Technical Constraints

- Web-only application targeting modern browsers
- Input text length: 5,000–50,000 characters
- Single Epic per AI generation session
- All task relationships within single project
- All users have identical permissions (no role differentiation in MVP)
- All status transitions allowed without restrictions

## Authentication

Auth0 integration required for both frontend (auth0-angular) and backend (JWT Bearer). Users must authenticate before accessing any task data. Task assignment in MVP limited to self-assignment only.

## References

- Product Requirements: `docs/prd.md`
- Technology Stack Details: `docs/tech-stack.md`