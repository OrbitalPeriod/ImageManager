# Frontend Architecture

## Overview
This document describes the architecture of the Anime Image Manager frontend application, built with Next.js 14 (App Router), TypeScript, Tailwind CSS, and React.

## Folder Structure

```
src/
├── app/                          # Next.js App Router pages
│   ├── (auth)/                   # Auth route group
│   │   ├── login/
│   │   │   └── page.tsx
│   │   └── register/
│   │       └── page.tsx
│   ├── (protected)/              # Protected route group
│   │   ├── images/
│   │   │   ├── page.tsx
│   │   │   └── [id]/
│   │   │       └── page.tsx
│   │   ├── upload/
│   │   │   └── page.tsx
│   │   └── settings/
│   │       └── page.tsx
│   ├── api/                      # API route handlers (if needed for proxying)
│   ├── layout.tsx
│   ├── page.tsx                  # Home page
│   ├── error.tsx                 # Global error boundary
│   └── not-found.tsx
├── components/                   # React components
│   ├── ui/                       # Reusable UI components (buttons, inputs, etc.)
│   ├── auth/                     # Auth-related components
│   ├── images/                   # Image-related components
│   ├── layout/                   # Layout components (header, footer, nav)
│   └── shared/                   # Shared components
├── lib/
│   ├── api/                      # API client and utilities
│   │   ├── client.ts             # Generated OpenAPI types (auto-generated)
│   │   ├── fetch-client.ts       # Configured openapi-fetch client
│   │   ├── server-client.ts      # Server-side API client
│   │   └── client-client.ts      # Client-side API client
│   ├── auth/                     # Authentication utilities
│   │   ├── context.tsx           # Auth context provider
│   │   ├── hooks.ts              # Auth hooks (useAuth, useUser)
│   │   └── middleware.ts         # Auth middleware for route protection
│   ├── errors/                   # Error handling
│   │   ├── types.ts              # Error type definitions
│   │   ├── handlers.ts           # Error handling utilities
│   │   └── boundaries.tsx        # Error boundary components
│   ├── hooks/                    # Custom React hooks
│   │   ├── use-api.ts            # Generic API hook
│   │   └── use-pagination.ts     # Pagination hook
│   └── utils/                    # Utility functions
│       ├── cn.ts                 # className utility (clsx/tailwind-merge)
│       └── format.ts             # Formatting utilities
└── types/                        # Global TypeScript types
    └── index.ts
```

## API Layer

### Architecture
- **Base Client**: Configured `openapi-fetch` client with base URL, credentials, and error handling
- **Server Client**: For Server Components and Server Actions (uses server-side cookies)
- **Client Client**: For client-side components (uses browser cookies)

### Configuration
- Base URL: Configured via `NEXT_PUBLIC_API_URL` environment variable
- Credentials: `include` to send cookies (cookie-based auth)
- Error handling: Centralized error transformation

### Usage Patterns
1. **Server Components**: Use `getServerClient()` for initial data fetching
2. **Server Actions**: Use `getServerClient()` for mutations
3. **Client Components**: Use `getClient()` for client-side data fetching

## Data Fetching Strategy

### Server Components (Default)
- Used for initial page loads and SEO-critical data
- Fetch data directly in Server Components using `await`
- No loading states needed (rendered on server)
- Example: List pages, image detail pages

### Client-Side Fetching
- Use React hooks (`useApi`, custom hooks) for:
  - Interactive data (search, filters)
  - Real-time updates
  - User-triggered actions
- Uses React Query or SWR patterns (fetch with useState/useEffect)

### Server Actions
- For mutations (create, update, delete)
- Handled via Server Actions for form submissions
- Automatic revalidation support

### Caching Strategy
- Server Components: Next.js automatic caching with `fetch`
- Client Components: Manual cache management or React Query
- Revalidation: On-demand via Server Actions or route handlers

## Authentication Handling

### Cookie-Based Authentication
The API uses cookie-based authentication (sessions):
- Login endpoint sets HTTP-only cookies
- Cookies are automatically included in requests with `credentials: 'include'`
- No token management needed on the client

### Auth Context
- `AuthProvider`: Wraps the app, provides auth state
- `useAuth()`: Hook to access auth state and actions
- `useUser()`: Hook to access current user data

### Route Protection
1. **Middleware**: Protect routes at the edge (next.js middleware)
2. **Route Groups**: Use `(protected)` group for protected routes
3. **Client-side**: Use `useAuth()` to check auth state

### User Session
- Fetch user data on app load via `/api/users/me`
- Cache user data in context
- Refresh on navigation to protected routes

## Error Handling

### Error Types
1. **API Errors**: HTTP errors from the API (4xx, 5xx)
2. **Validation Errors**: Form validation errors (400 with ErrorsResponse)
3. **Network Errors**: Network failures
4. **Auth Errors**: 401 Unauthorized (redirect to login)

### Error Handling Strategy
- **Server Components**: Use try/catch, redirect or show error UI
- **Client Components**: Use error boundaries and error states
- **Forms**: Show inline validation errors
- **Global Errors**: Use `error.tsx` for route-level errors

### Error Boundaries
- Route-level: `error.tsx` for each route
- Component-level: Error boundary components for critical sections
- Global: Root error boundary in layout

### Error Response Structure
```typescript
// ProblemDetails (standard API error)
{
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

// ErrorsResponse (validation errors)
{
  errors?: string[];
}
```

## Key Design Decisions

1. **API Client**: Use `openapi-fetch` for type-safe API calls
2. **Authentication**: Cookie-based, no token storage needed
3. **Data Fetching**: Prefer Server Components for initial load
4. **State Management**: React Context for auth, local state for UI
5. **Error Handling**: Multi-layer approach (boundaries, inline, global)
6. **Type Safety**: Leverage generated OpenAPI types throughout

## Environment Variables

```env
NEXT_PUBLIC_API_URL=http://localhost:8080  # Backend API URL
```

Create a `.env.local` file (or `.env`) with this variable. See `.env.example` for reference.

## Development Workflow

1. **API Changes**: Regenerate client types from `api.json`
2. **New Features**: Follow the folder structure, create components in appropriate directories
3. **Auth**: Use auth context and hooks for protected features
4. **Errors**: Handle errors at the appropriate level (component, route, global)
