/**
 * Generic API Hook
 * 
 * A custom hook for making API calls with loading and error state management.
 * This is a simple implementation - you might want to use React Query or SWR
 * for more advanced features like caching and refetching.
 */

'use client';

import { useState, useCallback, useEffect } from 'react';
import { handleError } from '@/lib/errors/handlers';
import type { AppError } from '@/lib/errors/types';

interface UseApiState<T> {
  data: T | null;
  loading: boolean;
  error: AppError | null;
}

interface UseApiReturn<T> extends UseApiState<T> {
  execute: () => Promise<T | null>;
  reset: () => void;
}

/**
 * Generic hook for API calls
 * 
 * @param apiCall - Async function that makes the API call
 * @param immediate - Whether to execute immediately on mount (default: false)
 */
export function useApi<T>(
  apiCall: () => Promise<T>,
  immediate: boolean = false
): UseApiReturn<T> {
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: immediate,
    error: null,
  });

  const execute = useCallback(async () => {
    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const result = await apiCall();
      setState({ data: result, loading: false, error: null });
      return result;
    } catch (error) {
      const appError = handleError(error);
      setState({ data: null, loading: false, error: appError });
      return null;
    }
  }, [apiCall]);

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  // Execute immediately if requested
  useEffect(() => {
    if (immediate) {
      execute();
    }
  }, [immediate, execute]);

  return {
    ...state,
    execute,
    reset,
  };
}
