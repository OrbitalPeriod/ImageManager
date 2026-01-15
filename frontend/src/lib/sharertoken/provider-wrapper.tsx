/**
 * ShareToken Provider Wrapper
 * 
 * Wraps ShareTokenProvider with Suspense boundary required for useSearchParams
 */

'use client';

import { Suspense, useEffect, useContext } from 'react';
import { useSearchParams } from 'next/navigation';
import { ShareTokenProvider } from './context';
import { ShareTokenContext } from './context';

// Component that reads search params and updates the provider
function ShareTokenSearchParamsSync({ children }: { children: React.ReactNode }) {
  const searchParams = useSearchParams();
  const context = useContext(ShareTokenContext);
  
  useEffect(() => {
    if (context?.updateFromSearchParams) {
      context.updateFromSearchParams(searchParams);
    }
  }, [searchParams, context]);

  return <>{children}</>;
}

export function ShareTokenProviderWrapper({ children }: { children: React.ReactNode }) {
  return (
    <ShareTokenProvider>
      <Suspense fallback={children}>
        <ShareTokenSearchParamsSync>{children}</ShareTokenSearchParamsSync>
      </Suspense>
    </ShareTokenProvider>
  );
}
