/**
 * Server-side API Client
 * 
 * Convenience wrapper for server-side API calls that automatically
 * handles cookie forwarding from Next.js requests.
 */

import { cookies } from 'next/headers';
import { getServerClient } from './fetch-client';

/**
 * Get an API client configured for server-side usage.
 * Automatically forwards cookies from the Next.js request.
 * 
 * This should be used in:
 * - Server Components
 * - Server Actions
 * - Route Handlers
 */
export async function getServerApiClient() {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore
    .getAll()
    .map((cookie) => `${cookie.name}=${cookie.value}`)
    .join('; ');
  return getServerClient(cookieHeader);
}
