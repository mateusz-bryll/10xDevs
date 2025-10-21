# REST API Plan for AI TaskFlow

## 1. Resources

### 1.1 Users Module
- **User** - Represents authenticated users (maps to `User` record and `UserId` value object)

### 1.2 Work Items Module
- **Project** - Represents a project container (maps to `Project` entity and `ProjectId` value object)
- **WorkItem** - Represents a task unit in the hierarchy (maps to `WorkItem` entity and `WorkItemId` value object)
- **WorkItemType** - Enum defining hierarchy levels: Epic, Story, Task
- **WorkItemStatus** - Enum defining task states: New, Ready, InProgress, Done

### 1.3 Assistant Module
- **GeneratedStructure** - AI-generated task hierarchy (transient, not persisted)
- **ApprovalRequest** - Batch of work items to create from AI generation

---

## 2. Endpoints

### 2.1 Users Module

#### 2.1.1 Get Current User
- **HTTP Method**: `GET`
- **URL Path**: `/api/users/me`
- **Backend Module**: `TaskFlow.Modules.Users`
- **Description**: Retrieves the currently authenticated user's profile information
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**: None
- **Response Payload**:
```json
{
  "id": "auth0|507f1f77bcf86cd799439011",
  "email": "user@example.com",
  "name": "John Doe",
  "picture": "https://example.com/avatar.jpg"
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `404 Not Found` - User profile not found in system

---

#### 2.1.2 Search Users by Email
- **HTTP Method**: `GET`
- **URL Path**: `/api/users/search`
- **Backend Module**: `TaskFlow.Modules.Users`
- **Description**: Searches for users by email address (full or partial match) for task assignment purposes. Returns maximum 5 results.
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**:
  - `email` (required, string, min 1 char) - Full or partial email address to search
- **Request Payload**: None
- **Response Payload**:
```json
{
  "users": [
    {
      "id": "auth0|507f1f77bcf86cd799439011",
      "email": "john.doe@example.com",
      "name": "John Doe",
      "picture": "https://example.com/avatar1.jpg"
    },
    {
      "id": "auth0|507f1f77bcf86cd799439012",
      "email": "john.smith@example.com",
      "name": "John Smith",
      "picture": "https://example.com/avatar2.jpg"
    }
  ],
  "total": 2
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `400 Bad Request` - Missing or invalid email parameter

---

### 2.2 Work Items Module - Projects

#### 2.2.1 List User Projects
- **HTTP Method**: `GET`
- **URL Path**: `/api/projects`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Retrieves paginated list of projects owned by the authenticated user (without work items)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**:
  - `page` (optional, integer, default: 1, min: 1) - Page number
  - `pageSize` (optional, integer, default: 20, min: 1, max: 100) - Items per page
- **Request Payload**: None
- **Response Payload**:
```json
{
  "projects": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "E-commerce Platform",
      "description": "Redesign of the main e-commerce platform"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 45,
    "totalPages": 3
  }
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `400 Bad Request` - Invalid pagination parameters

---

#### 2.2.2 Create Project
- **HTTP Method**: `POST`
- **URL Path**: `/api/projects`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Creates a new project owned by the authenticated user
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "name": "E-commerce Platform",
  "description": "Redesign of the main e-commerce platform"
}
```
- **Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "E-commerce Platform",
  "description": "Redesign of the main e-commerce platform",
  "ownerId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z"
}
```
- **Success Response**: `201 Created` with `Location` header pointing to new resource
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `400 Bad Request` - Validation errors (e.g., missing name, name too long)
- **Validation Rules**:
  - `name` - Required, max 200 characters
  - `description` - Optional, max 2000 characters

---

#### 2.2.3 Get Project by ID
- **HTTP Method**: `GET`
- **URL Path**: `/api/projects/{projectId}`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Retrieves a specific project by ID (user must be owner)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**: None
- **Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "E-commerce Platform",
  "description": "Redesign of the main e-commerce platform",
  "ownerId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-20T14:45:00Z"
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User is not the project owner
  - `404 Not Found` - Project does not exist

---

#### 2.2.4 Update Project
- **HTTP Method**: `PUT`
- **URL Path**: `/api/projects/{projectId}`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Updates an existing project (user must be owner)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "name": "E-commerce Platform v2",
  "description": "Updated description"
}
```
- **Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "E-commerce Platform v2",
  "description": "Updated description",
  "ownerId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-21T09:15:00Z"
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User is not the project owner
  - `404 Not Found` - Project does not exist
  - `400 Bad Request` - Validation errors
- **Validation Rules**:
  - `name` - Required, max 200 characters
  - `description` - Optional, max 2000 characters

---

#### 2.2.5 Delete Project
- **HTTP Method**: `DELETE`
- **URL Path**: `/api/projects/{projectId}`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Deletes a project and all its work items (cascade delete, user must be owner)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**: None
- **Response Payload**:
```json
{
  "message": "Project deleted successfully",
  "deletedCount": 47
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User is not the project owner
  - `404 Not Found` - Project does not exist

---

### 2.3 Work Items Module - Work Items

#### 2.3.1 List Work Items for Project
- **HTTP Method**: `GET`
- **URL Path**: `/api/projects/{projectId}/work-items`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Retrieves paginated list of work items for a project with optional filtering by parent (supports lazy loading for tree view). By default returns top-level Epics when parentId is not specified.
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**:
  - `parentId` (optional, guid) - Filter by parent ID. Omit or use `null` for root-level Epics. Provide Epic ID to load Stories, Story ID to load Tasks.
  - `page` (optional, integer, default: 1, min: 1) - Page number
  - `pageSize` (optional, integer, default: 20, min: 1, max: 100) - Items per page
- **Request Payload**: None
- **Response Payload**:
```json
{
  "workItems": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "parentId": null,
      "workItemType": "Epic",
      "title": "User Authentication System",
      "status": "InProgress",
      "assignedUserId": "auth0|507f1f77bcf86cd799439011",
      "hasChildren": true
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 3,
    "totalPages": 1
  }
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project does not exist
  - `400 Bad Request` - Invalid pagination or parentId parameters

---

#### 2.3.2 Get Work Item by ID
- **HTTP Method**: `GET`
- **URL Path**: `/api/projects/{projectId}/work-items/{workItemId}`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Retrieves a specific work item by ID with calculated progress
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**: None
- **Response Payload**:
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "parentId": null,
  "workItemType": "Epic",
  "title": "User Authentication System",
  "description": "Implement complete authentication flow with OAuth2 and JWT",
  "status": "InProgress",
  "assignedUserId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:35:00Z",
  "updatedAt": "2025-01-18T16:20:00Z",
  "progress": {
    "completed": 2,
    "total": 5,
    "percentage": 40
  },
  "hasChildren": true
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project or work item does not exist

---

#### 2.3.3 Create Work Item
- **HTTP Method**: `POST`
- **URL Path**: `/api/projects/{projectId}/work-items`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Creates a new work item in the project
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "parentId": null,
  "workItemType": "Epic",
  "title": "User Authentication System",
  "description": "Implement complete authentication flow",
  "assignedUserId": "auth0|507f1f77bcf86cd799439011"
}
```
- **Response Payload**:
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "parentId": null,
  "workItemType": "Epic",
  "title": "User Authentication System",
  "description": "Implement complete authentication flow",
  "status": "New",
  "assignedUserId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:35:00Z",
  "updatedAt": "2025-01-15T10:35:00Z",
  "progress": {
    "completed": 0,
    "total": 0,
    "percentage": 0
  },
  "hasChildren": false
}
```
- **Success Response**: `201 Created` with `Location` header
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project or parent work item does not exist
  - `400 Bad Request` - Validation errors
- **Validation Rules**:
  - `title` - Required, max 200 characters
  - `description` - Optional, max 5000 characters
  - `workItemType` - Required, must be "Epic", "Story", or "Task"
  - `parentId` - Must be null for Epic, must reference existing Epic for Story, must reference existing Story for Task
  - `assignedUserId` - Optional, must reference existing user if provided
- **Business Rules**:
  - Epic: `parentId` must be null
  - Story: `parentId` must point to an Epic
  - Task: `parentId` must point to a Story

---

#### 2.3.4 Update Work Item
- **HTTP Method**: `PUT`
- **URL Path**: `/api/projects/{projectId}/work-items/{workItemId}`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Updates an existing work item (title, description, parentId)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "title": "User Authentication System v2",
  "description": "Updated description with OAuth2 details",
  "parentId": null
}
```
- **Response Payload**:
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "parentId": null,
  "workItemType": "Epic",
  "title": "User Authentication System v2",
  "description": "Updated description with OAuth2 details",
  "status": "InProgress",
  "assignedUserId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:35:00Z",
  "updatedAt": "2025-01-21T11:30:00Z",
  "progress": {
    "completed": 2,
    "total": 5,
    "percentage": 40
  },
  "hasChildren": true
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project or work item does not exist
  - `400 Bad Request` - Validation errors
- **Validation Rules**:
  - Same as Create Work Item
- **Business Rules**:
  - Changing `parentId` must maintain hierarchy rules (Epic → Story → Task)
  - Cannot change `workItemType` (not included in update payload)

---

#### 2.3.5 Delete Work Item
- **HTTP Method**: `DELETE`
- **URL Path**: `/api/projects/{projectId}/work-items/{workItemId}`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Deletes a work item and all its children (cascade delete)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**: None
- **Response Payload**:
```json
{
  "message": "Work item deleted successfully",
  "deletedCount": 8
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project or work item does not exist
- **Business Rules**:
  - Deleting an Epic deletes all Stories and Tasks beneath it
  - Deleting a Story deletes all Tasks beneath it
  - Returns count of deleted items (including the parent)

---

#### 2.3.6 Update Work Item Status
- **HTTP Method**: `PUT`
- **URL Path**: `/api/projects/{projectId}/work-items/{workItemId}/status`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Updates the status of a work item (all transitions allowed per PRD)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "status": "InProgress"
}
```
- **Response Payload**:
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "parentId": null,
  "workItemType": "Epic",
  "title": "User Authentication System",
  "description": "Implement complete authentication flow",
  "status": "InProgress",
  "assignedUserId": "auth0|507f1f77bcf86cd799439011",
  "createdAt": "2025-01-15T10:35:00Z",
  "updatedAt": "2025-01-21T12:00:00Z",
  "progress": {
    "completed": 2,
    "total": 5,
    "percentage": 40
  },
  "hasChildren": true
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project or work item does not exist
  - `400 Bad Request` - Invalid status value
- **Validation Rules**:
  - `status` - Required, must be one of: "New", "Ready", "InProgress", "Done"
- **Business Rules**:
  - All status transitions are allowed (no restrictions per PRD)

---

#### 2.3.7 Assign Work Item to User
- **HTTP Method**: `PUT`
- **URL Path**: `/api/projects/{projectId}/work-items/{workItemId}/assign`
- **Backend Module**: `TaskFlow.Modules.WorkItems`
- **Description**: Assigns a work item to a user (or unassigns if userId is null)
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "userId": "auth0|507f1f77bcf86cd799439012"
}
```
- **Response Payload**:
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "parentId": null,
  "workItemType": "Epic",
  "title": "User Authentication System",
  "description": "Implement complete authentication flow",
  "status": "InProgress",
  "assignedUserId": "auth0|507f1f77bcf86cd799439012",
  "createdAt": "2025-01-15T10:35:00Z",
  "updatedAt": "2025-01-21T12:15:00Z",
  "progress": {
    "completed": 2,
    "total": 5,
    "percentage": 40
  },
  "hasChildren": true
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project, work item, or assigned user does not exist
  - `400 Bad Request` - Invalid userId format
- **Validation Rules**:
  - `userId` - Optional (null to unassign), must reference existing user if provided
- **Business Rules**:
  - User can assign work items to themselves or other users (no restriction in MVP)

---

### 2.4 Assistant Module

#### 2.4.1 Generate Work Items from Markdown
- **HTTP Method**: `POST`
- **URL Path**: `/api/assistant/generate`
- **Backend Module**: `TaskFlow.Modules.Assistant`
- **Description**: Generates a structured task hierarchy (Epics → Stories → Tasks) from Markdown input using AI. Returns the generated structure without persisting it (client-side draft).
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "markdownInput": "# E-commerce Platform\n\n## User Stories\n\n### Authentication\n- User login\n- User registration\n- Password reset\n\n### Shopping Cart\n- Add items to cart\n- Remove items from cart\n- Update quantities\n- Checkout process",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
- **Response Payload**:
```json
{
  "epics": [
    {
      "workItemType": "Epic",
      "title": "E-commerce Platform Implementation",
      "description": "Complete implementation of e-commerce platform features",
      "stories": [
        {
          "workItemType": "Story",
          "title": "User Authentication",
          "description": "Implement user authentication flow",
          "tasks": [
            {
              "workItemType": "Task",
              "title": "Implement user login",
              "description": "Create login form and API endpoint"
            },
            {
              "workItemType": "Task",
              "title": "Implement user registration",
              "description": "Create registration form and API endpoint"
            },
            {
              "workItemType": "Task",
              "title": "Implement password reset",
              "description": "Create password reset flow with email verification"
            }
          ]
        },
        {
          "workItemType": "Story",
          "title": "Shopping Cart Management",
          "description": "Implement shopping cart functionality",
          "tasks": [
            {
              "workItemType": "Task",
              "title": "Add items to cart",
              "description": "Allow users to add products to shopping cart"
            },
            {
              "workItemType": "Task",
              "title": "Remove items from cart",
              "description": "Allow users to remove products from cart"
            },
            {
              "workItemType": "Task",
              "title": "Update cart quantities",
              "description": "Allow users to change product quantities"
            },
            {
              "workItemType": "Task",
              "title": "Implement checkout process",
              "description": "Create checkout flow with payment integration"
            }
          ]
        }
      ]
    }
  ],
  "metadata": {
    "totalEpics": 1,
    "totalStories": 2,
    "totalTasks": 7,
    "generatedAt": "2025-01-21T14:30:00Z"
  }
}
```
- **Success Response**: `200 OK`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project does not exist
  - `400 Bad Request` - Validation errors (e.g., markdown too short/long, invalid format)
  - `422 Unprocessable Entity` - AI generation failed or produced invalid structure
  - `500 Internal Server Error` - AI service unavailable
- **Validation Rules**:
  - `markdownInput` - Required, min 5000 characters, max 50000 characters
  - `projectId` - Required, must reference existing project owned by user
- **Business Rules**:
  - Does NOT persist generated structure (client manages draft state)
  - Can be called multiple times (regenerate) without side effects

---

#### 2.4.2 Approve and Create Work Items
- **HTTP Method**: `POST`
- **URL Path**: `/api/assistant/approve`
- **Backend Module**: `TaskFlow.Modules.Assistant`
- **Description**: Creates approved work items from AI-generated structure in a single transaction. Accepts full or partial structure (supports "approve all" and "approve selected" workflows).
- **Authentication**: Required (JWT Bearer token)
- **Query Parameters**: None
- **Request Payload**:
```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "epics": [
    {
      "workItemType": "Epic",
      "title": "E-commerce Platform Implementation",
      "description": "Complete implementation of e-commerce platform features",
      "stories": [
        {
          "workItemType": "Story",
          "title": "User Authentication",
          "description": "Implement user authentication flow",
          "tasks": [
            {
              "workItemType": "Task",
              "title": "Implement user login",
              "description": "Create login form and API endpoint"
            },
            {
              "workItemType": "Task",
              "title": "Implement user registration",
              "description": "Create registration form and API endpoint"
            }
          ]
        }
      ]
    }
  ]
}
```
- **Response Payload**:
```json
{
  "createdWorkItems": {
    "epics": 1,
    "stories": 1,
    "tasks": 2,
    "total": 4
  },
  "createdAt": "2025-01-21T14:45:00Z",
  "workItemIds": [
    "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "8d9e6679-7425-40de-944b-e07fc1f90ae8",
    "9e9e6679-7425-40de-944b-e07fc1f90ae9",
    "ae9e6679-7425-40de-944b-e07fc1f90aea"
  ]
}
```
- **Success Response**: `201 Created`
- **Error Responses**:
  - `401 Unauthorized` - Invalid or missing authentication token
  - `403 Forbidden` - User does not own the project
  - `404 Not Found` - Project does not exist
  - `400 Bad Request` - Validation errors (e.g., invalid hierarchy, missing required fields)
  - `409 Conflict` - Epic with same title already exists in project
- **Validation Rules**:
  - `projectId` - Required, must reference existing project owned by user
  - `epics` - Required array, must contain at least one epic
  - `epic.title` - Required, max 200 characters
  - `epic.description` - Optional, max 5000 characters
  - `epic.stories` - Optional array (can be empty for Epic only)
  - `story.title` - Required, max 200 characters
  - `story.tasks` - Optional array (can be empty for Story only)
  - `task.title` - Required, max 200 characters
- **Business Rules**:
  - Entire structure created in a single database transaction (all or nothing)
  - Epics must not have a parentId (automatically set to null)
  - Stories automatically linked to respective Epic via parentId
  - Tasks automatically linked to respective Stories via parentId
  - All items default to status "New"
  - All items initially unassigned (assignedUserId = null)

---

## 3. Authentication and Authorization

### 3.1 Authentication Mechanism
**Auth0 JWT Bearer Token Authentication**

All API endpoints require authentication via Auth0-issued JWT Bearer tokens in the `Authorization` header:

```
Authorization: Bearer <jwt_token>
```

### 3.2 Implementation Details

#### Backend (.NET Core 9)
- **Library**: `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Configuration**: `appsettings.json`
  ```json
  {
    "Auth0": {
      "Domain": "YOUR_AUTH0_DOMAIN",
      "Audience": "YOUR_AUTH0_AUDIENCE",
      "Authority": "https://YOUR_AUTH0_DOMAIN/"
    }
  }
  ```
- **Middleware**: JWT Bearer middleware validates tokens on every request
- **Claims**: Extract `UserId` from token claims (Auth0 `sub` claim)

#### Frontend (Angular 20)
- **Library**: `@auth0/auth0-angular`
- **Token Management**: Auth0 SDK handles token acquisition, refresh, and storage
- **HTTP Interceptor**: Automatically attaches Bearer token to API requests

### 3.3 Authorization Rules

#### Project Ownership
- Users can only access projects they own (`OwnerId` matches authenticated `UserId`)
- Enforced on all project and work item endpoints
- Returns `403 Forbidden` if user attempts to access another user's project

#### User Search
- Users can search all users in the system (no ownership restriction)
- Required for task assignment functionality

#### Work Item Assignment
- Users can assign work items to any user in the system (no restriction in MVP)
- Future: May be restricted to project members

### 3.4 Error Responses
- `401 Unauthorized` - Missing, invalid, or expired JWT token
- `403 Forbidden` - Valid token but insufficient permissions (e.g., accessing another user's project)

---

## 4. Validation and Business Logic

### 4.1 Project Validation

#### Create/Update Project
- **Name**: Required, max 200 characters, cannot be empty or whitespace only
- **Description**: Optional, max 2000 characters
- **OwnerId**: Automatically set from authenticated user (not in request payload)

#### Delete Project
- Cascade deletes all work items in the project
- Returns `deletedCount` (total work items deleted) for user confirmation
- No undo mechanism in MVP

---

### 4.2 Work Item Validation

#### General Validation
- **Title**: Required, max 200 characters, cannot be empty or whitespace only
- **Description**: Optional, max 5000 characters
- **WorkItemType**: Required, must be one of: "Epic", "Story", "Task" (case-insensitive)
- **Status**: Must be one of: "New", "Ready", "InProgress", "Done" (case-insensitive)
- **ProjectId**: Required, must reference existing project
- **ParentId**: Required for Story and Task, must be null for Epic
- **AssignedUserId**: Optional, must reference existing user if provided

#### Hierarchy Validation (Critical Business Rule)
The API enforces a **strict three-level hierarchy** (Epic → Story → Task):

| Work Item Type | Parent Type | Parent ID Requirement |
|----------------|-------------|----------------------|
| **Epic** | None | Must be `null` |
| **Story** | Epic | Must reference an existing Epic |
| **Task** | Story | Must reference an existing Story |

**Validation Logic**:
1. **Epic Creation**:
   - `parentId` must be null
   - If `parentId` is provided, return `400 Bad Request`: "Epic cannot have a parent"

2. **Story Creation**:
   - `parentId` must be provided
   - Parent must exist and be of type "Epic"
   - If parent is not Epic, return `400 Bad Request`: "Story must have an Epic as parent"

3. **Task Creation**:
   - `parentId` must be provided
   - Parent must exist and be of type "Story"
   - If parent is not Story, return `400 Bad Request`: "Task must have a Story as parent"

4. **Update ParentId**:
   - Same validation rules apply when updating `parentId` via PUT
   - Cannot change work item type (not allowed in update)
   - Must maintain hierarchy integrity

---

### 4.3 Status Transition Logic

Per PRD requirement: **"All status transitions allowed without restrictions"**

- No validation on status changes (e.g., can go from "New" directly to "Done")
- Status changes tracked via `UpdatedAt` timestamp
- Future: May add workflow rules, but not in MVP

**Allowed Statuses**:
- `New` (0) - Initial state for all new work items
- `Ready` (1) - Work item ready to start
- `InProgress` (2) - Work item currently being worked on
- `Done` (3) - Work item completed

---

### 4.4 Assignment Logic

Per PRD: **"Users can assign tasks to themselves"** (US-013)

**MVP Rules**:
- Any authenticated user can assign any work item to any user (no restrictions)
- Assignment is optional (work items can be unassigned)
- To assign: provide `userId` in request
- To unassign: provide `null` for `userId`
- Invalid `userId` returns `404 Not Found`

**Future Enhancements** (Out of Scope for MVP):
- Restrict assignment to project members only
- Notify assigned user via email/notification
- Track assignment history

---

### 4.5 Progress Calculation Logic

Per PRD: **"Parent progress shown as numeric ratio (e.g., 2/10) and progress bar"**

**Calculation Rules**:
1. **For Leaf Items** (Tasks with no children):
   - `completed`: 0
   - `total`: 0
   - `percentage`: 0
   - Display: "-/-" with grayed-out progress bar

2. **For Parent Items** (Epics and Stories with children):
   - `completed`: Count of children with `status = "Done"`
   - `total`: Total count of direct children
   - `percentage`: `(completed / total) * 100` (rounded to integer)
   - Display: "2/10" with progress bar at 20%

3. **Recursive Progress** (Not in MVP):
   - MVP only tracks direct children progress
   - Does not calculate nested progress (e.g., Epic progress based on Task completion)
   - Future: May add recursive/weighted progress calculation

**Example**:
- Epic has 5 Stories (2 Done, 3 InProgress)
  - `completed`: 2
  - `total`: 5
  - `percentage`: 40
  - Display: "2/5" with 40% progress bar

---

### 4.6 Cascade Deletion Logic

Per PRD: **"Cascade deletion ensures all child tasks are removed when a parent task is deleted"**

**Deletion Behavior**:
1. **Delete Epic**:
   - Deletes all Stories under the Epic
   - Deletes all Tasks under those Stories
   - Returns total count (Epic + Stories + Tasks)

2. **Delete Story**:
   - Deletes all Tasks under the Story
   - Returns total count (Story + Tasks)

3. **Delete Task**:
   - Deletes only the Task (no children)
   - Returns count of 1

**Implementation**:
- Database-level cascade delete via foreign key constraints (EF Core)
- API returns `deletedCount` in response for user confirmation/feedback
- Deletion is permanent (no soft delete in MVP)

**Error Handling**:
- If work item doesn't exist, return `404 Not Found`
- If user doesn't own project, return `403 Forbidden`

---

### 4.7 AI Generation Validation

Per PRD: **"Input text length: 5,000–50,000 characters"**

#### Generate Endpoint Validation
- **markdownInput**:
  - Required
  - Minimum: 5000 characters
  - Maximum: 50000 characters
  - Must be valid UTF-8 text
  - If outside range, return `400 Bad Request`: "Markdown input must be between 5,000 and 50,000 characters"

- **projectId**:
  - Required
  - Must reference existing project owned by authenticated user
  - If invalid, return `404 Not Found` or `403 Forbidden`

#### Approve Endpoint Validation
- **epics**: Required array, must contain at least one epic
- **stories**: Optional array within each epic
- **tasks**: Optional array within each story
- Validates hierarchy: Epics → Stories → Tasks
- If structure invalid, return `400 Bad Request` with detailed error message

**Business Rules**:
- Generate endpoint does NOT persist data (stateless, client-side draft)
- Approve endpoint creates all items in a single transaction
- If transaction fails, no items are created (atomic operation)
- Duplicate title check: If Epic with same title exists in project, return `409 Conflict`

---

### 4.8 Pagination Logic

**Standardized Pagination**:
- **page**: Default 1, minimum 1
- **pageSize**: Default 20, minimum 1, maximum 100
- If invalid values provided, return `400 Bad Request`

**Response Format**:
```json
{
  "items": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

**Performance Consideration**:
- Use offset-based pagination for MVP (simple, sufficient for expected data volumes)
- Future: May migrate to cursor-based pagination for better performance

---

## 5. Error Response Format

All error responses follow the **RFC 7807 Problem Details** standard with `Content-Type: application/problem+json`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "The request contains invalid data",
  "instance": "/api/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/work-items",
  "errors": {
    "title": [
      "Title is required and cannot be empty"
    ],
    "parentId": [
      "Story must have an Epic as parent"
    ]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Problem Details Fields**:
- `type` (string, URI) - URI reference identifying the problem type
- `title` (string) - Short, human-readable summary of the problem
- `status` (integer) - HTTP status code
- `detail` (string, optional) - Human-readable explanation specific to this occurrence
- `instance` (string, URI) - URI reference identifying the specific occurrence
- `errors` (object, optional) - Validation errors grouped by field name (ASP.NET Core format)
- `traceId` (string, optional) - Request trace identifier for debugging

**Standard Problem Types**:
- `400 Bad Request` - Validation errors, invalid input
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict (e.g., duplicate Epic title)
- `422 Unprocessable Entity` - Business logic violation
- `500 Internal Server Error` - Server error

**Example Error Responses**:

**Validation Error (400)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "title": ["Title is required and cannot be empty"],
    "workItemType": ["WorkItemType must be Epic, Story, or Task"]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Authentication Error (401)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid or missing authentication token",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Authorization Error (403)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to access this project",
  "instance": "/api/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Not Found Error (404)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Project with ID '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found",
  "instance": "/api/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Conflict Error (409)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "An Epic with the title 'E-commerce Platform Implementation' already exists in this project",
  "instance": "/api/assistant/approve",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

---

## 6. Performance and Scalability Considerations

### 6.1 Pagination
- All list endpoints support pagination to prevent large payloads
- Default page size: 20 items
- Maximum page size: 100 items

### 6.2 Lazy Loading
- Work items endpoint supports `parentId` filtering for tree view lazy loading
- Reduces initial payload size by loading only root-level Epics
- Child items loaded on demand when user expands tree nodes

### 6.3 Progress Calculation
- Progress calculated on-demand during GET requests (not pre-computed)
- Only counts direct children (not recursive in MVP)
- Cached at database level via indexed queries

### 6.4 Database Indexing (Recommendations)
- Index on `ProjectId` for work items (frequent filtering)
- Index on `ParentId` for work items (lazy loading queries)
- Index on `OwnerId` for projects (user-specific queries)
- Composite index on `ProjectId + ParentId` for optimal tree queries

### 6.5 Caching Strategy (Future)
- Redis cache for user profiles (Auth0 data)
- No caching in MVP (manual refresh model per PRD)
- Future: Cache project lists and work item trees with invalidation on updates

---

## 7. API Versioning Strategy

**MVP Approach**: No versioning (breaking changes avoided)

**Future Strategy**:
- URL-based versioning: `/api/v1/projects`, `/api/v2/projects`
- Version introduced only when breaking changes required
- Maintain backward compatibility for at least 6 months after new version release

---

## 8. Rate Limiting (Future Enhancement)

**Not implemented in MVP**, but recommended for production:
- AI generation endpoint: 10 requests per minute per user
- Other endpoints: 100 requests per minute per user
- Returns `429 Too Many Requests` with `Retry-After` header

---

## 9. CORS Configuration

**Frontend Origin**: `http://localhost:4200` (development), production domain (deployment)

**Allowed Methods**: `GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`

**Allowed Headers**: `Authorization`, `Content-Type`, `Accept`

**Credentials**: Allowed (for cookies if needed in future)

---

## 10. Summary

This API plan provides a **comprehensive, RESTful interface** for the AI TaskFlow MVP, focusing on:

1. **Simplicity**: Minimal endpoints covering core functionality
2. **Consistency**: Standardized request/response formats, error handling, and pagination
3. **Security**: Auth0 JWT authentication on all endpoints, project ownership validation
4. **Flexibility**: Support for both AI-generated and manual task workflows
5. **Scalability**: Pagination, lazy loading, and hierarchical data structure
6. **Alignment**: Direct mapping to PRD requirements and domain model

The API is designed to support the Angular frontend with clear contracts, comprehensive validation, and detailed error messages, enabling rapid MVP development while maintaining extensibility for future enhancements.
