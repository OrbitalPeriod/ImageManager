/**
 * Error Types and Utilities
 * 
 * Type definitions for API errors and error handling utilities.
 */

import type { components } from '@/lib/api/client';

type ProblemDetails = components['schemas']['ProblemDetails'];
type ErrorsResponse = components['schemas']['ErrorsResponse'];
type ErrorResponse = components['schemas']['ErrorResponse'];

/**
 * API Error - Represents an error returned from the API
 */
export class ApiError extends Error {
  status: number;
  detail?: string;
  title?: string;
  type?: string;
  instance?: string;
  errors?: string[]; // Validation errors

  constructor(
    status: number,
    message: string,
    details?: ProblemDetails | ErrorsResponse | ErrorResponse
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;

    if ('detail' in (details || {})) {
      const problemDetails = details as ProblemDetails;
      this.detail = problemDetails.detail || undefined;
      this.title = problemDetails.title || undefined;
      this.type = problemDetails.type || undefined;
      this.instance = problemDetails.instance || undefined;
    }

    if ('errors' in (details || {})) {
      const errorsResponse = details as ErrorsResponse;
      this.errors = errorsResponse.errors || undefined;
    }

    if ('message' in (details || {})) {
      const errorResponse = details as ErrorResponse;
      this.message = errorResponse.message || message;
    }
  }

  /**
   * Check if this is an authentication error (401)
   */
  isAuthError(): boolean {
    return this.status === 401;
  }

  /**
   * Check if this is a validation error (400 with errors array)
   */
  isValidationError(): boolean {
    return this.status === 400 && Array.isArray(this.errors) && this.errors.length > 0;
  }

  /**
   * Check if this is a client error (4xx)
   */
  isClientError(): boolean {
    return this.status >= 400 && this.status < 500;
  }

  /**
   * Check if this is a server error (5xx)
   */
  isServerError(): boolean {
    return this.status >= 500;
  }
}

/**
 * Network Error - Represents a network failure
 */
export class NetworkError extends Error {
  constructor(message: string = 'Network error occurred') {
    super(message);
    this.name = 'NetworkError';
  }
}

/**
 * Unknown Error - Represents an unexpected error
 */
export class UnknownError extends Error {
  originalError: unknown;

  constructor(message: string, originalError?: unknown) {
    super(message);
    this.name = 'UnknownError';
    this.originalError = originalError;
  }
}

/**
 * Error type union
 */
export type AppError = ApiError | NetworkError | UnknownError;
