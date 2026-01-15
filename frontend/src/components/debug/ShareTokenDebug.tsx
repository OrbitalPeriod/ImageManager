/**
 * ShareToken Debug Component
 * 
 * Displays current sharetoken state for testing/debugging purposes.
 * Remove this component in production.
 */

'use client';

import { useShareToken } from '@/lib/sharertoken/hooks';
import { useEffect, useState } from 'react';

export function ShareTokenDebug() {
  const { token } = useShareToken();
  const [sessionToken, setSessionToken] = useState<string | null>(null);

  useEffect(() => {
    // Read directly from sessionStorage to verify sync
    try {
      setSessionToken(sessionStorage.getItem('sharetoken'));
    } catch (error) {
      setSessionToken(null);
    }
  }, [token]);

  if (process.env.NODE_ENV === 'production') {
    return null; // Don't show in production
  }

  return (
    <div className="fixed bottom-4 right-4 bg-card border border-border rounded-lg p-4 shadow-lg z-50 text-xs font-mono max-w-sm">
      <div className="font-bold mb-2 text-primary">ShareToken Debug</div>
      <div className="space-y-1">
        <div>
          <span className="text-muted-foreground">Context Token:</span>{' '}
          <span className={token ? 'text-green-500' : 'text-red-500'}>
            {token || 'null'}
          </span>
        </div>
        <div>
          <span className="text-muted-foreground">SessionStorage:</span>{' '}
          <span className={sessionToken ? 'text-green-500' : 'text-red-500'}>
            {sessionToken || 'null'}
          </span>
        </div>
        <div>
          <span className="text-muted-foreground">URL Token:</span>{' '}
          <span className="text-blue-500">
            {typeof window !== 'undefined'
              ? new URLSearchParams(window.location.search).get('token') || 'null'
              : 'null'}
          </span>
        </div>
        <div className="mt-2 pt-2 border-t border-border">
          <div className="text-muted-foreground text-[10px]">
            Check Network tab to verify token is included in API requests
          </div>
        </div>
      </div>
    </div>
  );
}
