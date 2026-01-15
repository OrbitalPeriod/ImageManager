/**
 * ShareToken Context
 * 
 * Provides sharetoken state management throughout the application.
 * Token is read from URL query parameters with sessionStorage fallback.
 */

'use client';

import React, { createContext, useContext, useEffect, useState, useCallback, useRef } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';

interface ShareTokenContextType {
  token: string | null;
  setToken: (token: string | null) => void;
  clearToken: () => void;
  updateFromSearchParams?: (params: URLSearchParams) => void;
}

export const ShareTokenContext = createContext<ShareTokenContextType | undefined>(undefined);

const STORAGE_KEY = 'sharetoken';

interface ShareTokenProviderProps {
  children: React.ReactNode;
}

/**
 * ShareTokenProvider - Provides sharetoken context to the application
 * 
 * Token priority:
 * 1. URL query parameter `token` (highest priority)
 * 2. sessionStorage (fallback)
 */
export function ShareTokenProvider({ children }: ShareTokenProviderProps) {
  const router = useRouter();
  const [token, setTokenState] = useState<string | null>(() => {
    // Initialize from sessionStorage on mount (client-side only)
    if (typeof window !== 'undefined') {
      try {
        return sessionStorage.getItem(STORAGE_KEY);
      } catch (error) {
        console.warn('Failed to read token from sessionStorage:', error);
        return null;
      }
    }
    return null;
  });
  const [isInitialized, setIsInitialized] = useState(false);
  const prevUrlTokenRef = useRef<string | null>(null);
  const searchParamsRef = useRef<URLSearchParams | null>(null);

  // This will be set by the wrapper component that has access to searchParams
  const setSearchParams = useCallback((params: URLSearchParams) => {
    searchParamsRef.current = params;
    
    if (!isInitialized) {
      // First, check URL query params (highest priority)
      const urlToken = params.get('token');
      
      if (urlToken) {
        // Token in URL - use it and store in sessionStorage
        setTokenState(urlToken);
        prevUrlTokenRef.current = urlToken;
        try {
          sessionStorage.setItem(STORAGE_KEY, urlToken);
        } catch (error) {
          console.warn('Failed to store token in sessionStorage:', error);
        }
      } else {
        // No token in URL - check sessionStorage (already initialized above)
        prevUrlTokenRef.current = null;
      }

      setIsInitialized(true);
    }
  }, [isInitialized]);

  // Sync token changes to sessionStorage and URL
  const setToken = useCallback((newToken: string | null) => {
    setTokenState(newToken);

    // Update sessionStorage
    try {
      if (newToken) {
        sessionStorage.setItem(STORAGE_KEY, newToken);
      } else {
        sessionStorage.removeItem(STORAGE_KEY);
      }
    } catch (error) {
      console.warn('Failed to update token in sessionStorage:', error);
    }

    // Update URL query params
    const params = searchParamsRef.current 
      ? new URLSearchParams(searchParamsRef.current.toString())
      : new URLSearchParams();
    if (newToken) {
      params.set('token', newToken);
    } else {
      params.delete('token');
    }

    const newUrl = params.toString() ? `?${params.toString()}` : window.location.pathname;
    router.replace(newUrl, { scroll: false });
  }, [router]);

  const clearToken = useCallback(() => {
    setToken(null);
  }, [setToken]);

  // Watch for URL changes (e.g., when user navigates or token is changed externally)
  const updateFromSearchParams = useCallback((params: URLSearchParams) => {
    if (!isInitialized) {
      setSearchParams(params);
      return;
    }

    const urlToken = params.get('token');
    
    // Only update if URL token actually changed (not just state change)
    if (urlToken !== prevUrlTokenRef.current) {
      prevUrlTokenRef.current = urlToken;
      
      if (urlToken) {
        // URL has token - use it (highest priority)
        setTokenState(urlToken);
        try {
          sessionStorage.setItem(STORAGE_KEY, urlToken);
        } catch (error) {
          console.warn('Failed to store token in sessionStorage:', error);
        }
      } else {
        // Token removed from URL - fallback to sessionStorage if available
        try {
          const storedToken = sessionStorage.getItem(STORAGE_KEY);
          if (storedToken) {
            // sessionStorage has token - use it as fallback
            setTokenState(storedToken);
          } else {
            // No token in URL or sessionStorage - clear state
            setTokenState(null);
          }
        } catch (error) {
          console.warn('Failed to read token from sessionStorage:', error);
          // If we can't read sessionStorage and URL has no token, clear state
          setTokenState(null);
        }
      }
    }
  }, [isInitialized, setSearchParams]);

  // Expose updateFromSearchParams via context for the wrapper to use
  const value: ShareTokenContextType = {
    token,
    setToken,
    clearToken,
    updateFromSearchParams,
  };

  return <ShareTokenContext.Provider value={value}>{children}</ShareTokenContext.Provider>;
}

/**
 * useShareToken - Hook to access sharetoken context
 */
export function useShareToken(): ShareTokenContextType {
  const context = useContext(ShareTokenContext);
  if (context === undefined) {
    throw new Error('useShareToken must be used within a ShareTokenProvider');
  }
  return context;
}
