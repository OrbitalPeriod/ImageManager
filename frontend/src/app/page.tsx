/**
 * Homepage
 * Based on the example Index design, adapted for Next.js and API integration
 */

'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleApiResponseError, handleError, getErrorMessage } from '@/lib/errors/handlers';
import { useDebounce } from '@/lib/hooks/useDebounce';
import { TopNavbar } from '@/components/layout/TopNavbar';
import { SearchSection, type AgeRating } from '@/components/images/SearchSection';
import { ImageGallery, type ImageData } from '@/components/images/ImageGallery';
import { PaginationControls } from '@/components/images/PaginationControls';
import { ErrorPopup } from '@/components/ui/ErrorPopup';

interface SearchImagesResponse {
  data?: ImageData[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export default function Home() {
  // Search state
  const [characterSearch, setCharacterSearch] = useState('');
  const [tagSearch, setTagSearch] = useState('');
  const debouncedCharacterSearch = useDebounce(characterSearch, 400);
  const debouncedTagSearch = useDebounce(tagSearch, 400);

  // Filter state
  const [selectedRatings, setSelectedRatings] = useState<AgeRating[]>([]);

  // Pagination state
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(24);

  // Data state
  const [images, setImages] = useState<ImageData[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Get API base URL from environment
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || '';

  // Parse search strings into arrays (split by comma)
  const parseSearchTerms = (searchString: string): string[] => {
    return searchString
      .split(',')
      .map((term) => term.trim())
      .filter((term) => term.length > 0);
  };

  // Fetch images from API
  const fetchImages = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const client = getClientApiClient();

      // Prepare query parameters
      const characters = parseSearchTerms(debouncedCharacterSearch);
      const tags = parseSearchTerms(debouncedTagSearch);
      const ratings = selectedRatings.length > 0 ? selectedRatings : undefined;

      // Build query params
      const queryParams: {
        page: number;
        pageSize: number;
        Characters?: string[];
        Tags?: string[];
        Rating?: AgeRating[];
      } = {
        page,
        pageSize,
      };

      if (characters.length > 0) {
        queryParams.Characters = characters;
      }
      if (tags.length > 0) {
        queryParams.Tags = tags;
      }
      if (ratings && ratings.length > 0) {
        queryParams.Rating = ratings;
      }

      const response = await client.GET('/api/images/search', {
        params: {
          query: queryParams,
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setImages([]);
        setTotalPages(1);
        return;
      }

      const data = response.data as SearchImagesResponse | undefined;
      if (data) {
        setImages(data.data || []);
        setTotalPages(data.totalPages || 1);
      } else {
        setImages([]);
        setTotalPages(1);
      }
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      setImages([]);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  }, [debouncedCharacterSearch, debouncedTagSearch, selectedRatings, page, pageSize]);

  // Fetch images when dependencies change
  useEffect(() => {
    fetchImages();
  }, [fetchImages]);

  // Reset to page 1 when filters change
  useEffect(() => {
    if (page !== 1) {
      setPage(1);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedCharacterSearch, debouncedTagSearch, selectedRatings]);

  // Handle rating toggle
  const handleRatingChange = (rating: AgeRating) => {
    setSelectedRatings((prev) => {
      if (prev.includes(rating)) {
        return prev.filter((r) => r !== rating);
      } else {
        return [...prev, rating];
      }
    });
  };

  // Handle page size change
  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setPage(1); // Reset to first page
  };

  return (
    <div className="min-h-screen bg-background">
      <TopNavbar />
      <main>
        <SearchSection
          characterSearch={characterSearch}
          tagSearch={tagSearch}
          selectedRatings={selectedRatings}
          onCharacterSearchChange={setCharacterSearch}
          onTagSearchChange={setTagSearch}
          onRatingChange={handleRatingChange}
        />
        {loading ? (
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading images...</div>
          </div>
        ) : (
          <>
            <ImageGallery images={images} apiBaseUrl={apiBaseUrl} />
            <PaginationControls
              currentPage={page}
              totalPages={totalPages}
              pageSize={pageSize}
              onPageChange={setPage}
              onPageSizeChange={handlePageSizeChange}
            />
          </>
        )}
      </main>

      {error && (
        <ErrorPopup
          message={error}
          onClose={() => setError(null)}
        />
      )}
    </div>
  );
}