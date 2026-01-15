# Architecture Implementation Guide

This document provides a quick reference for using the frontend architecture.

## Quick Start

### 1. Environment Variables

Create a `.env.local` file (or copy `.env.example`):

```env
NEXT_PUBLIC_API_URL=http://localhost:8080
```

**Note:** In Next.js, environment variables prefixed with `NEXT_PUBLIC_` are exposed to the browser, which is required for client-side API calls.

### 2. Setting Up Auth Provider

Wrap your app with the `AuthProvider` in `src/app/layout.tsx`:

```tsx
import { AuthProvider } from '@/lib/auth/context';

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html>
      <body>
        <AuthProvider>
          {children}
        </AuthProvider>
      </body>
    </html>
  );
}
```

### 3. API Client Usage

#### Server Components (Server-side data fetching)

```tsx
import { getServerApiClient } from '@/lib/api/server-client';

export default async function ImagesPage() {
  const client = await getServerApiClient();
  const response = await client.GET('/api/images', {
    params: {
      query: { page: 1, pageSize: 20 }
    }
  });

  if (response.error) {
    // Handle error
    return <div>Error loading images</div>;
  }

  const images = response.data?.data || [];
  return <div>{/* Render images */}</div>;
}
```

#### Client Components (Client-side data fetching)

```tsx
'use client';

import { getClientApiClient } from '@/lib/api/client-client';
import { useApi } from '@/lib/hooks/use-api';

export function ImagesList() {
  const { data, loading, error, execute } = useApi(async () => {
    const client = getClientApiClient();
    const response = await client.GET('/api/images');
    if (response.error) throw response.error;
    return response.data;
  }, true); // true = execute immediately

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  return <div>{/* Render data */}</div>;
}
```

#### Server Actions (Mutations)

```tsx
'use server';

import { getServerApiClient } from '@/lib/api/server-client';
import { revalidatePath } from 'next/cache';

export async function deleteImage(imageId: string) {
  const client = await getServerApiClient();
  const response = await client.DELETE('/api/images/{imageId}', {
    params: { path: { imageId } }
  });

  if (response.error) {
    return { error: 'Failed to delete image' };
  }

  revalidatePath('/images');
  return { success: true };
}
```

### 4. Authentication

#### Using Auth Context

```tsx
'use client';

import { useAuth, useUser } from '@/lib/auth/hooks';

export function UserProfile() {
  const { user, isLoading, logout } = useAuth();
  // Or use convenience hook:
  // const user = useUser();

  if (isLoading) return <div>Loading...</div>;
  if (!user) return <div>Not logged in</div>;

  return (
    <div>
      <p>Welcome, {user.userName}</p>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

#### Login Example

```tsx
'use client';

import { getClientApiClient } from '@/lib/api/client-client';
import { useAuth } from '@/lib/auth/hooks';
import { useRouter } from 'next/navigation';

export function LoginForm() {
  const { refreshUser } = useAuth();
  const router = useRouter();

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    
    const client = getClientApiClient();
    const response = await client.POST('/api/auth/login', {
      body: {
        username: formData.get('username') as string,
        password: formData.get('password') as string,
      }
    });

    if (response.error) {
      // Handle error
      alert('Login failed');
      return;
    }

    // Refresh user data
    await refreshUser();
    router.push('/');
  }

  return <form onSubmit={handleSubmit}>{/* Form fields */}</form>;
}
```

### 5. Error Handling

#### Server Components

```tsx
import { getServerApiClient } from '@/lib/api/server-client';
import { handleError, getErrorMessage } from '@/lib/errors/handlers';

export default async function ImagesPage() {
  try {
    const client = await getServerApiClient();
    const response = await client.GET('/api/images');
    
    if (response.error) {
      throw response.error;
    }
    
    return <div>{/* Render data */}</div>;
  } catch (error) {
    const message = getErrorMessage(error);
    return <div>Error: {message}</div>;
  }
}
```

#### Client Components with Error Boundaries

```tsx
'use client';

import { ErrorBoundary } from '@/lib/errors/boundaries';

export function ImagesPage() {
  return (
    <ErrorBoundary>
      <ImagesList />
    </ErrorBoundary>
  );
}
```

### 6. Route Protection

Create `src/middleware.ts`:

```tsx
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const isAuthenticated = request.cookies.has('your-auth-cookie-name');
  const { pathname } = request.nextUrl;

  // Protect routes
  const protectedPaths = ['/upload', '/settings'];
  if (protectedPaths.some(path => pathname.startsWith(path)) && !isAuthenticated) {
    const url = request.nextUrl.clone();
    url.pathname = '/login';
    url.searchParams.set('redirect', pathname);
    return NextResponse.redirect(url);
  }

  // Redirect authenticated users away from auth pages
  if ((pathname === '/login' || pathname === '/register') && isAuthenticated) {
    return NextResponse.redirect(new URL('/', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    '/((?!api|_next/static|_next/image|favicon.ico).*)',
  ],
};
```

## Key Files Created

### API Layer
- `src/lib/api/fetch-client.ts` - Base API client configuration
- `src/lib/api/server-client.ts` - Server-side API client
- `src/lib/api/client-client.ts` - Client-side API client

### Authentication
- `src/lib/auth/context.tsx` - Auth context provider
- `src/lib/auth/hooks.ts` - Auth hooks (useAuth, useUser, etc.)
- `src/lib/auth/middleware.ts` - Middleware utilities (reference only)

### Error Handling
- `src/lib/errors/types.ts` - Error type definitions
- `src/lib/errors/handlers.ts` - Error handling utilities
- `src/lib/errors/boundaries.tsx` - Error boundary components

### Hooks & Utilities
- `src/lib/hooks/use-api.ts` - Generic API hook
- `src/lib/hooks/use-pagination.ts` - Pagination hook
- `src/lib/utils/cn.ts` - className utility

## Next Steps

1. **Update the root layout** to include `AuthProvider`
2. **Create middleware** at `src/middleware.ts` for route protection
3. **Set up environment variables** for the API URL
4. **Create route groups** following the folder structure in `ARCHITECTURE.md`
5. **Implement components** using the patterns shown above

## Notes

- The API uses cookie-based authentication, so cookies are automatically included
- Server Components should use `getServerApiClient()` for data fetching
- Client Components should use `getClientApiClient()` or the `useApi` hook
- Error handling is centralized - use the error utilities for consistent error handling
- The `openapi-fetch` client is type-safe based on your `api.json` schema
