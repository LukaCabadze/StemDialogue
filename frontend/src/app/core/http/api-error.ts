import { HttpErrorResponse } from '@angular/common/http';

export type ApiErrorKind = 'not-found' | 'offline' | 'server';

/** Classifies a failed request so pages can show "not found" separately from generic errors. */
export function apiErrorKind(error: unknown): ApiErrorKind {
  const cause = error instanceof Error && 'cause' in error ? (error as { cause?: unknown }).cause : undefined;
  const response = error instanceof HttpErrorResponse ? error : cause instanceof HttpErrorResponse ? cause : null;

  if (response?.status === 404) {
    return 'not-found';
  }
  if (response?.status === 0) {
    return 'offline';
  }
  return 'server';
}

export function isNotFound(error: unknown): boolean {
  return apiErrorKind(error) === 'not-found';
}
