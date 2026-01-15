/**
 * Authentication Context
 * 
 * Provides authentication state and actions throughout the application.
 */

'use client';

import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import type { components } from '@/lib/api/client';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleError } from '@/lib/errors/handlers';
import { ApiError } from '@/lib/errors/types';

type User = components['schemas']['GetUserInfoResponse'];

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  refreshUser: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: React.ReactNode;
  initialUser?: User | null;
}

/**
 * AuthProvider - Provides authentication context to the application
 */
export function AuthProvider({ children, initialUser = null }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(initialUser);
  const [isLoading, setIsLoading] = useState(!initialUser);

  const fetchUser = useCallback(async () => {
    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/users/me');

      if (response.data) {
        setUser(response.data);
      } else {
        setUser(null);
      }
    } catch (error) {
      // If 401, user is not authenticated
      const appError = handleError(error);
      if (appError instanceof ApiError && appError.isAuthError()) {
        setUser(null);
      } else {
        // For other errors, we might want to keep the existing user state
        // or handle it differently
        console.error('Failed to fetch user:', error);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  const refreshUser = useCallback(async () => {
    setIsLoading(true);
    await fetchUser();
  }, [fetchUser]);

  const logout = useCallback(async () => {
    try {
      const client = getClientApiClient();
      await client.POST('/api/auth/logout');
      setUser(null);
      // Optionally redirect to login page
      window.location.href = '/login';
    } catch (error) {
      console.error('Logout failed:', error);
      // Even if logout fails, clear local state
      setUser(null);
      window.location.href = '/login';
    }
  }, []);

  useEffect(() => {
    // Fetch user on mount if not provided initially
    if (!initialUser) {
      fetchUser();
    }
  }, [initialUser, fetchUser]);

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated: !!user,
    refreshUser,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/**
 * useAuth - Hook to access authentication context
 */
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
