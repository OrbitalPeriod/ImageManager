/**
 * Query parameters for the `/api/images` endpoint.
 */
export interface ImageQueryParams {
  /** UUID token used for authentication/authorization */
  token: string;
  /** Zero‑based page index */
  page: number;
  /** Number of items per page */
  pageSize: number;
}

/**
 * Fetch images from the backend.
 *
 * @param params Query parameters for pagination and auth.
 * @returns The parsed JSON response.  The exact shape depends on your API – you can type it later.
 */
export async function fetchImages(params: ImageQueryParams): Promise<any> {
    const baseUrl = import.meta.env.VITE_API_URL ?? window.location.origin;

  const url = new URL('/api/images', baseUrl);

  // Append query string parameters
  url.searchParams.append('token', params.token);
  url.searchParams.append('page', String(params.page));
  url.searchParams.append('pageSize', String(params.pageSize));

  const response = await fetch(url.toString(), {
    method: 'GET',
    headers: {
      Accept: 'application/json',
    },
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch images: ${response.status} ${response.statusText}`);
  }

  return response.json();
}