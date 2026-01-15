/**
 * Error Handling Utilities
 * 
 * Functions for handling and transforming API errors.
 */

import type { paths } from '@/lib/api/client';
import { ApiError, NetworkError, UnknownError, type AppError } from './types';

/**
 * Generic API response structure from openapi-fetch
 */
type ApiResponse<T = unknown> = {
  data?: T;
  error?: unknown;
  response: Response;
};

/**
 * Handle an API response and throw appropriate errors if needed.
 * 
 * @param response - The API response from openapi-fetch
 * @returns The response data if successful
 * @throws ApiError if the API returned an error
 */
export function handleApiResponse<T>(
  response: ApiResponse<T>
): T {
  if (response.error) {
    throw createApiError(response);
  }

  if (!response.data) {
    throw new ApiError(response.response.status, 'No data returned from API');
  }

  return response.data;
}

/**
 * Create an ApiError from an openapi-fetch error response.
 */
function createApiError(response: ApiResponse): ApiError {
  const { response: httpResponse, error } = response;
  const status = httpResponse?.status || 0;

  if (!status) {
    // Network error or no response
    throw new NetworkError('Failed to connect to the API');
  }

  // The error from openapi-fetch is already parsed JSON body
  const errorDetails = error;

  // Default status-based messages
  const defaultMessage = status === 401
    ? 'Unauthorized. Please log in.'
    : status === 403
    ? 'Forbidden. You do not have permission to access this resource.'
    : status === 404
    ? 'Resource not found.'
    : status >= 500
    ? 'Server error. Please try again later.'
    : 'An error occurred';

  // Check if error has a message field (ErrorResponse format)
  // openapi-fetch returns the parsed JSON body in the error field
  // For 401 with {"message":"Login failed"}, error should be { message: "Login failed" }
  let message = defaultMessage;
  if (errorDetails && typeof errorDetails === 'object' && errorDetails !== null) {
    const errorObj = errorDetails as Record<string, unknown>;
    
    // Check for ErrorResponse format: { message: "..." }
    if ('message' in errorObj && typeof errorObj.message === 'string' && errorObj.message.trim()) {
      message = errorObj.message;
    }
    // Check for ProblemDetails format: { detail: "..." }
    else if ('detail' in errorObj && typeof errorObj.detail === 'string' && errorObj.detail.trim()) {
      message = errorObj.detail;
    }
    // Check for ProblemDetails format: { title: "..." }
    else if ('title' in errorObj && typeof errorObj.title === 'string' && errorObj.title.trim()) {
      message = errorObj.title;
    }
  }

  return new ApiError(status, message, errorDetails as any);
}

/**
 * Handle an openapi-fetch response that contains an error.
 * This should be used when response.error is present.
 * 
 * @param response - The openapi-fetch response with an error
 * @returns An AppError representing the API error
 */
export function handleApiResponseError<T>(response: ApiResponse<T>): AppError {
  if (response.error) {
    return createApiError(response);
  }
  // If no error but we're calling this function, something is wrong
  return new UnknownError('An unexpected error occurred');
}

/**
 * Handle an error and transform it into an AppError.
 * Useful for catch blocks to ensure consistent error types.
 */
export function handleError(error: unknown): AppError {
  if (error instanceof ApiError || error instanceof NetworkError || error instanceof UnknownError) {
    return error;
  }

  if (error instanceof Error) {
    // Network errors often show up as TypeError or generic Error
    if (error.message.includes('fetch') || error.message.includes('network')) {
      return new NetworkError(error.message);
    }
    return new UnknownError(error.message, error);
  }

  return new UnknownError('An unexpected error occurred', error);
}

/**
 * Get user-friendly error message from an error.
 */
export function getErrorMessage(error: unknown): string {
  const appError = handleError(error);

  if (appError instanceof ApiError) {
    // For ApiError, prefer detail, title, or message (in that order)
    // The message field may contain ErrorResponse.message which should be shown
    return appError.detail || appError.title || appError.message;
  }

  return appError.message;
}

/**
 * Get validation errors from an error.
 * Returns undefined if the error is not a validation error.
 */
export function getValidationErrors(error: unknown): string[] | undefined {
  const appError = handleError(error);
  if (appError instanceof ApiError && appError.isValidationError()) {
    return appError.errors;
  }
  return undefined;
}
