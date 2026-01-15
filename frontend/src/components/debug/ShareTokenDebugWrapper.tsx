/**
 * ShareToken Debug Wrapper
 * Client component wrapper for the debug component
 */

'use client';

import { ShareTokenDebug } from './ShareTokenDebug';

export function ShareTokenDebugWrapper() {
  // Only show in development
  if (process.env.NODE_ENV === 'production') {
    return null;
  }
  
  return <ShareTokenDebug />;
}
