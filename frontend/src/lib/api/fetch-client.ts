/**
 * API Client Configuration
 * 
 * This file configures the openapi-fetch client for both server and client usage.
 * The API uses cookie-based authentication, so credentials are always included.
 */

import createClient from 'openapi-fetch';
import type { paths } from './client';

/**
 * Get the API base URL from environment variables.
 * Falls back to empty string for relative URLs (same-origin).
 */
function getApiBaseUrl(): string {
  if (typeof window !== 'undefined') {
    // Client-side: use public env var or relative URL
    return process.env.NEXT_PUBLIC_API_URL || '';
  }
  // Server-side: use env var or default
  return process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || '';
}

/**
 * Creates a configured API client instance.
 * 
 * @param fetchImpl - Optional fetch implementation (useful for server-side with custom config)
 */
export function createApiClient(fetchImpl?: typeof fetch) {
  const baseUrl = getApiBaseUrl();

  const client = createClient<paths>({
    baseUrl,
    credentials: 'include', // Always include cookies for cookie-based auth
    fetch: fetchImpl,
  });

  return client;
}

/**
 * Get a client-side API client instance.
 * This instance will use browser cookies and the browser's fetch.
 */
export function getClient() {
  if (typeof window === 'undefined') {
    throw new Error('getClient() can only be used on the client side. Use getServerClient() on the server.');
  }
  return createApiClient();
}

/**
 * Get a server-side API client instance.
 * This instance will forward cookies from the request and use server-side fetch.
 * 
 * @param cookies - Cookie header string from the request (for forwarding cookies)
 */
export function getServerClient(cookies?: string) {
  if (typeof window !== 'undefined') {
    throw new Error('getServerClient() can only be used on the server side. Use getClient() on the client.');
  }

  const baseUrl = getApiBaseUrl();

  // Create a fetch implementation that forwards cookies
  const fetchImpl: typeof fetch = async (input, init) => {
    const headers = new Headers(init?.headers);

    // Forward cookies from the request
    if (cookies) {
      headers.set('cookie', cookies);
    }

    const url = typeof input === 'string' 
      ? (input.startsWith('http') ? input : `${baseUrl}${input}`)
      : input;

    return fetch(url, {
      ...init,
      headers,
      credentials: 'include',
    });
  };

  return createApiClient(fetchImpl);
}
