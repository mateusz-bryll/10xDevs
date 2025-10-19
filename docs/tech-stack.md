Frontend - Angular SPA:
- Angular 20 will provide interactivity where needed
- TypeScript 5 for static typing and better IDE support
- Tailwind 4 allows for convenient application styling
- PrimeNG 20 provides ready-made components (tree view, Kanban, forms) that align perfectly with PRD requirements

Backend - .NET Core:
- .NET Core 9 and C# will provide a stable solution for building REST API using ASP.NET Core Minimal APIs
- .NET Core Minimal APIs are streamlined for quick REST API development
- Has well-tested libraries for building applications
- Allows deployment in Docker environment
- Is a popular solution that can be easily run locally or on a server

Database - PostgreSQL:
- Stable SQL database
- Has an adapter for Entity Framework Core
- Handles millions of records

Authentication and Authorization - Auth0:
- Has libraries for integration with .NET and Angular applications
- Allows managing users and registration of new accounts
- Provides API for managing user database
- Provides secure OAuth authorization server that generates JWT tokens
- Eliminates need to build authentication from scratch

AI - Communication with models through Semantic Kernel:
- Access to models from OpenAI
- Provides good abstraction for AI model integration

CI/CD and Hosting:
- Github Actions for creating CI/CD pipelines
- Hostinger for hosting the application via docker image (requires docker-compose.yaml file in repo)