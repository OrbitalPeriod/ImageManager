/**
 * Authentication Hooks
 * 
 * Convenience hooks for authentication functionality.
 */

'use client';

import { useAuth } from './context';
import type { components } from '@/lib/api/client';

type User = components['schemas']['GetUserInfoResponse'];

/**
 * useUser - Hook to access the current user
 * 
 * @returns The current user object, or null if not authenticated
 */
export function useUser(): User | null {
  const { user } = useAuth();
  return user;
}

/**
 * useIsAuthenticated - Hook to check if the user is authenticated
 * 
 * @returns true if the user is authenticated, false otherwise
 */
export function useIsAuthenticated(): boolean {
  const { isAuthenticated } = useAuth();
  return isAuthenticated;
}

/**
 * useRequireAuth - Hook that redirects to login if not authenticated
 * 
 * This is a convenience hook that checks authentication and can be used
 * in components that require authentication.
 * 
 * @param redirectTo - Optional path to redirect to after login (default: '/login')
 */
export function useRequireAuth(redirectTo: string = '/login'): void {
  const { isAuthenticated, isLoading } = useAuth();

  if (!isLoading && !isAuthenticated) {
    // In a real app, you might want to use next/navigation's redirect
    // For now, we'll use window.location
    if (typeof window !== 'undefined') {
      window.location.href = redirectTo;
    }
  }
}
