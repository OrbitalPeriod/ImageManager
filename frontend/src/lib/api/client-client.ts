/**
 * Client-side API Client
 * Uses openapi-fetch for type-safe API calls from client components
 */

import createClient, { type Middleware } from 'openapi-fetch';
import type { paths } from './client';

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || '';
const STORAGE_KEY = 'sharetoken';

// Middleware to automatically add sharetoken to requests
const shareTokenMiddleware: Middleware = {
  async onRequest({ request }) {
    // Read token from sessionStorage (synced by ShareTokenContext)
    try {
      const token = sessionStorage.getItem(STORAGE_KEY);
      if (token) {
        const url = new URL(request.url);
        // Only add token if it doesn't already exist in query params
        if (!url.searchParams.has('token')) {
          url.searchParams.set('token', token);
          return new Request(url.toString(), request);
        }
      }
    } catch (error) {
      // sessionStorage might not be available (SSR, private browsing, etc.)
      console.warn('Failed to read token from sessionStorage:', error);
    }
    return request;
  },
};

let clientInstance: ReturnType<typeof createClient<paths>> | null = null;

export function getClientApiClient() {
  // Create singleton client instance with middleware
  if (!clientInstance) {
    clientInstance = createClient<paths>({
      baseUrl: apiBaseUrl,
      credentials: 'include', // Include cookies for authentication
    });
    clientInstance.use(shareTokenMiddleware);
  }
  return clientInstance;
}
