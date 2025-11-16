// src/api/characters.ts
import type {
  GetCharacterResponsePaginatedResponse,
} from '../types/character';
import { apiFetch, baseUrl } from './api';


/**
 * Query options for `/api/characters`
 */
export interface CharacterQueryParams {

  token?: string;
  page?: number;      // defaults to 1 in the spec
  pageSize?: number;  // defaults to 20 in the spec
}

/**
 * Query options for `/api/characters/search`
 */
export interface CharacterSearchParams extends CharacterQueryParams {
  q?: string;        
}

/** GET /api/characters */
export async function getCharacters(
  params: CharacterQueryParams = {}
): Promise<GetCharacterResponsePaginatedResponse> {
  const url = new URL('/api/characters', baseUrl);

  if (params.token) url.searchParams.set('token', params.token);
  if (params.page !== undefined) url.searchParams.set('page', String(params.page));
  if (params.pageSize !== undefined)
    url.searchParams.set('pageSize', String(params.pageSize));

  const res = await apiFetch(url.toString(), { method: 'GET' });

  if (!res.ok) {
    throw new Error(
      `Failed to fetch characters (${res.status} ${res.statusText})`
    );
  }

  return res.json() as Promise<GetCharacterResponsePaginatedResponse>;
}

/** GET /api/characters/search */
export async function searchCharacters(
  params: CharacterSearchParams = {}
): Promise<GetCharacterResponsePaginatedResponse> {
  const url = new URL('/api/characters/search', baseUrl);

  if (params.q !== undefined) url.searchParams.set('q', params.q);
  if (params.token) url.searchParams.set('token', params.token);
  if (params.page !== undefined) url.searchParams.set('page', String(params.page));
  if (params.pageSize !== undefined)
    url.searchParams.set('pageSize', String(params.pageSize));

  const res = await apiFetch(url.toString(), { method: 'GET' });

  if (!res.ok) {
    throw new Error(
      `Failed to search characters (${res.status} ${res.statusText})`
    );
  }

  return res.json() as Promise<GetCharacterResponsePaginatedResponse>;
}
