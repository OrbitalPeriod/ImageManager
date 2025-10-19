// src/types/api.ts

/**
 * A single item returned by the API.
 */
export interface RatingItem {
  /** UUID of the item */
  id: string;
  /** Current rating – 0‑5, but can be any number if the API changes */
  rating: number;
}

/**
 * The full payload that your endpoint returns.
 *
 * @property data      Array of {@link RatingItem}s
 * @property page       Current page index (zero‑based)
 * @property pageSize   Number of items per page
 * @property totalItems Total number of items across all pages
 * @property totalPages  Total number of pages available
 */
export interface RatingsResponse {
  data: RatingItem[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

/**
 * A sample payload that matches the shape above.
 *
 * You can import this in tests or as a mock for your API client.
 */
export const sampleResponse: RatingsResponse = {
  data: [
    {
      id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      rating: 0,
    },
  ],
  page: 0,
  pageSize: 0,
  totalItems: 0,
  totalPages: 0,
};