/**
 * Authentication Middleware
 * 
 * Next.js middleware for protecting routes and handling authentication.
 * 
 * Note: This is a placeholder. The actual middleware should be created at
 * src/middleware.ts (not in lib) for Next.js to pick it up.
 * 
 * See: https://nextjs.org/docs/app/building-your-application/routing/middleware
 */

import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

/**
 * Check if a route requires authentication
 */
export function isProtectedRoute(pathname: string): boolean {
  const protectedPaths = ['/upload', '/settings', '/images/upload'];
  return protectedPaths.some((path) => pathname.startsWith(path));
}

/**
 * Check if a route is an auth route (login, register)
 */
export function isAuthRoute(pathname: string): boolean {
  const authPaths = ['/login', '/register'];
  return authPaths.includes(pathname);
}

/**
 * Middleware function to handle authentication
 * 
 * This should be exported from src/middleware.ts in your Next.js app.
 * 
 * Example usage in src/middleware.ts:
 * 
 * import { NextResponse } from 'next/server';
 * import type { NextRequest } from 'next/server';
 * 
 * export function middleware(request: NextRequest) {
 *   // Check for auth cookie
 *   const isAuthenticated = request.cookies.has('auth-cookie-name');
 *   const { pathname } = request.nextUrl;
 * 
 *   if (isProtectedRoute(pathname) && !isAuthenticated) {
 *     const url = request.nextUrl.clone();
 *     url.pathname = '/login';
 *     url.searchParams.set('redirect', pathname);
 *     return NextResponse.redirect(url);
 *   }
 * 
 *   if (isAuthRoute(pathname) && isAuthenticated) {
 *     const url = request.nextUrl.clone();
 *     url.pathname = '/';
 *     return NextResponse.redirect(url);
 *   }
 * 
 *   return NextResponse.next();
 * }
 * 
 * export const config = {
 *   matcher: [
 *     '/((?!api|_next/static|_next/image|favicon.ico).*)',
 *   ],
 * };
 */
