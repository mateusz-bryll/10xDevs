# Product Requirements Document (PRD) - AI TaskFlow

## 1. Product Overview

AI TaskFlow is an AI-assisted task management system inspired by GitHub Issues, designed to streamline project planning by automatically generating structured task hierarchies from textual business requirements. The platform focuses on simplifying project setup and reducing manual task creation through intelligent parsing and generation of Epics, Stories, and Tasks. 

The system enables users to input structured Markdown text representing project requirements, from which AI generates a task hierarchy following the structure: Epic → Story → Task. Users can manually review, edit, and approve AI-generated tasks before they are added to the main task list. Once approved, these tasks can be tracked, edited, and managed via a tree view or Kanban board interface.

The MVP (Minimum Viable Product) will focus on core functionalities—AI generation, approval workflow, manual task management, and task tracking—targeted for web platforms only.

## 2. User Problem

Creating and managing project tasks manually is a time-consuming process, particularly during the initial project planning phase. Product and engineering managers often translate business requirement documents into hundreds of detailed technical tasks. This process is repetitive, error-prone, and inefficient.

AI TaskFlow addresses this problem by:
- Automating the breakdown of project requirements into a structured, hierarchical set of tasks.
- Allowing users to review, refine, and approve AI-generated outputs before committing them to the main project board.
- Combining both AI-generated and manually created tasks in a unified interface to streamline management and reduce duplication.

The solution aims to save significant time during the project setup phase and ensure that teams can focus more on execution rather than administrative overhead.

## 3. Functional Requirements

### 3.1 Core Features

1. **AI Task Generation**
   - Manual trigger via a "Generate" button in a dedicated AI view.
   - Accepts structured Markdown input.
   - Generates one Epic per session, with up to 10 Stories and 100 Tasks.
   - Displays results in a collapsible tree preview before approval.
   - Allows full regeneration of the task hierarchy via "Regenerate all" with confirmation.

2. **Task Approval Flow**
   - Generated tasks remain in draft mode until approved (only on UI side).
   - Users can approve all or selected items via checkboxes.
   - Approved tasks are transferred to the main project task list (via dedicated endpoint).

3. **Task Management**
   - Manual creation, editing, and deletion of tasks.
   - Tasks follow fixed hierarchy (Epic → Story → Task).
   - Cascade deletion ensures all child tasks are removed when a parent task is deleted.
   - Separate edit forms for modifications (no inline editing).
   - All status transitions allowed without restrictions.

4. **Progress Tracking**
   - Parent progress shown as numeric ratio (e.g., 2/10) and progress bar.
   - If no children exist, progress appears as “-/-” with grayed-out bar.
   - Manual refresh button available for each Kanban or list view.

5. **User Management**
   - Users must authenticate and have unique accounts.
   - Tasks can be assigned to users manually.
   - Both managers and developers have identical permissions for MVP.

6. **UI & UX Design**
   - Tree view and Kanban board interfaces.
   - Manual refresh model (no auto-sync).
   - Toast notifications and inline messages for feedback.
   - Confirmation dialogs for major actions (e.g., regenerate all, delete parent task).
   - Web-only application.

7. **System Constraints**
   - Input text length: 5,000–50,000 characters.
   - Single Epic per AI generation.
   - No live synchronization; refresh is manual.
   - No analytics in MVP.

### 3.2 Technical and Design Assumptions
- All task relationships are contained within a single project.
- The system stores draft AI outputs separately until approval.
- The LLM integration will be via an API, abstracted from front-end.
- Cascading deletions prompt user confirmation before execution.
- Task naming conventions and input validations will be applied during implementation.

## 4. Product Boundaries

### 4.1 In Scope
- AI-assisted task generation from structured Markdown.
- Manual task creation, editing, and deletion.
- Task hierarchy visualization in both tree and Kanban views.
- User authentication and basic task assignment (only to self).
- Manual approval and regeneration of AI-generated tasks.
- Progress tracking through numeric indicators and progress bars.
- Manual refresh for synchronization.

### 4.2 Out of Scope for MVP
- Integration with external systems (e.g., GitHub Issues, Jira).
- Import of non-text formats (PDF, DOCX, etc.).
- Cross-project relationships between tasks.
- Mobile applications.
- Analytics, success metric dashboards, or detailed audit logging.

## 5. User Stories

### AI Generation & Approval Flow

**US-001 - Generate AI Tasks**  
*Description:*  
As a user, I want to generate a structured task hierarchy from a Markdown input so that I can quickly create project tasks without manual effort.  
*Acceptance Criteria:*  
- User provides valid Markdown text.  
- Clicking "Generate" triggers AI generation.  
- System displays results in a collapsible tree preview.  

**US-002 - Regenerate Task Structure**  
*Description:*  
As a user, I want to regenerate all AI-generated tasks so I can update the structure if the initial output is unsatisfactory.  
*Acceptance Criteria:*  
- “Regenerate all” prompts confirmation.  
- Regeneration replaces previous draft completely.  
- User sees updated preview after regeneration.

**US-003 - Approve All Generated Tasks**  
*Description:*  
As a user, I want to approve all AI-generated tasks at once so I can move them into the live project list efficiently.  
*Acceptance Criteria:*  
- “Approve all” button visible in AI view.  
- All draft items move to main task list upon approval.  
- Confirmation message displayed.

**US-004 - Approve Selected Tasks**  
*Description:*  
As a user, I want to selectively approve AI-generated tasks so I can control which tasks are added to the project.  
*Acceptance Criteria:*  
- Each generated task has a checkbox.  
- User can approve selected tasks.  
- Approved tasks move to main list; unapproved are discarded.

**US-005 - Reject or Delete Draft Tasks**  
*Description:*  
As a user, I want to delete AI-generated drafts that I do not wish to approve so that my workspace stays clean.  
*Acceptance Criteria:*  
- User can delete draft tasks individually.  
- Confirmation dialog appears before deletion.  
- Deleted drafts are permanently removed.

---

### Task Management

**US-006 - Create Manual Task**  
*Description:*  
As a user, I want to create a new task manually to add missing details or additional work items.  
*Acceptance Criteria:*  
- “Add Task” button is available in the main task list.  
- New task can be assigned hierarchy (Epic, Story, or Task).  
- System validates required fields (title, description).

**US-007 - Edit Task Details**  
*Description:*  
As a user, I want to edit existing task details so I can update information as the project evolves.  
*Acceptance Criteria:*  
- Editing occurs in a separate edit form.  
- User can modify title, description, assignee, and status.  
- Changes saved and reflected after submission.

**US-008 - Delete Task and Children**  
*Description:*  
As a user, I want to delete a task and its children so I can remove obsolete work items.  
*Acceptance Criteria:*  
- Confirmation dialog appears before cascade delete.  
- All child tasks removed upon deletion of parent.  
- System displays success notification.

**US-009 - Change Task Status**  
*Description:*  
As a user, I want to update the task status freely so I can track work progress.  
*Acceptance Criteria:*  
- All status transitions allowed.  
- Status changes visible immediately.  
- Toast notification confirms update.

---

### Progress Tracking

**US-010 - View Task Progress**  
*Description:*  
As a user, I want to view task progress visually and numerically so I can assess completion levels.  
*Acceptance Criteria:*  
- Progress displayed as ratio (e.g., 2/10).  
- Progress bar displayed for all parent tasks.  
- Tasks without children show grayed-out “-/-”.

**US-011 - Refresh Kanban View**  
*Description:*  
As a user, I want to manually refresh the Kanban view to see the most recent updates.  
*Acceptance Criteria:*  
- Refresh button near each Kanban/list view.  
- Clicking refresh reloads task data.  
- Confirmation of successful refresh via toast.

---

### User Management and Security

**US-012 - User Authentication**  
*Description:*  
As a user, I want to securely sign in and access only my assigned projects and tasks.  
*Acceptance Criteria:*  
- Authentication required before accessing dashboard.  
- System supports user registration and login.  
- Only authenticated users can view or edit tasks.

**US-013 - Task Assignment**  
*Description:*  
As a user, I want to assign tasks to myself so responsibility is clear.  
*Acceptance Criteria:*  
- “Assignee” field available in task form.  
- User can assign self.  
- Assigned user visible in task card.

---

### Notifications and Feedback

**US-014 - Receive Confirmation Feedback**  
*Description:*  
As a user, I want to receive immediate visual feedback (toasts or inline messages) when performing actions so I know the system’s response.  
*Acceptance Criteria:*  
- Toasts for success or error events.  
- Inline messages for validation errors.  
- Notifications disappear automatically after a few seconds.
