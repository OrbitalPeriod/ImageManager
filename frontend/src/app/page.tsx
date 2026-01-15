/**
 * Homepage
 * Based on the example Index design, adapted for Next.js and API integration
 */

'use client';

import React, { useState, useEffect, useCallback, useRef, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
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

function HomeContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const isInitialized = useRef(false);

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

  // Initialize state from URL parameters on mount
  useEffect(() => {
    if (isInitialized.current) return;
    
    const characters = searchParams.get('characters') || '';
    const tags = searchParams.get('tags') || '';
    const ratingsParam = searchParams.get('ratings');
    const pageParam = searchParams.get('page');
    const pageSizeParam = searchParams.get('pageSize');

    if (characters) setCharacterSearch(characters);
    if (tags) setTagSearch(tags);
    
    if (ratingsParam) {
      const ratings = ratingsParam
        .split(',')
        .map((r) => parseInt(r.trim(), 10))
        .filter((r) => !isNaN(r) && [0, 1, 2, 3].includes(r as AgeRating)) as AgeRating[];
      if (ratings.length > 0) {
        setSelectedRatings(ratings);
      }
    }

    if (pageParam) {
      const pageNum = parseInt(pageParam, 10);
      if (!isNaN(pageNum) && pageNum >= 1) {
        setPage(pageNum);
      }
    }

    if (pageSizeParam) {
      const pageSizeNum = parseInt(pageSizeParam, 10);
      if (!isNaN(pageSizeNum) && pageSizeNum >= 1) {
        setPageSize(pageSizeNum);
      }
    }

    isInitialized.current = true;
  }, [searchParams]);

  // Update URL parameters when state changes
  useEffect(() => {
    if (!isInitialized.current) return;

    const params = new URLSearchParams();

    if (debouncedCharacterSearch) {
      params.set('characters', debouncedCharacterSearch);
    }
    if (debouncedTagSearch) {
      params.set('tags', debouncedTagSearch);
    }
    if (selectedRatings.length > 0) {
      params.set('ratings', selectedRatings.join(','));
    }
    if (page > 1) {
      params.set('page', page.toString());
    }
    if (pageSize !== 24) {
      params.set('pageSize', pageSize.toString());
    }

    const newUrl = params.toString() ? `?${params.toString()}` : '/';
    const currentUrl = searchParams.toString() ? `?${searchParams.toString()}` : '/';
    
    if (newUrl !== currentUrl) {
      router.replace(newUrl, { scroll: false });
    }
  }, [debouncedCharacterSearch, debouncedTagSearch, selectedRatings, page, pageSize, router, searchParams]);

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
        // Sort images by storedAt (newest first)
        const sortedImages = (data.data || []).sort((a, b) => {
          // If both have storedAt, sort by date (newest first)
          if (a.storedAt && b.storedAt) {
            return new Date(b.storedAt).getTime() - new Date(a.storedAt).getTime();
          }
          // If only one has storedAt, prioritize it
          if (a.storedAt && !b.storedAt) return -1;
          if (!a.storedAt && b.storedAt) return 1;
          // If neither has storedAt, maintain original order
          return 0;
        });
        setImages(sortedImages);
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
    if (isInitialized.current) {
      fetchImages();
    }
  }, [fetchImages]);

  // Reset to page 1 when filters change
  useEffect(() => {
    if (!isInitialized.current) return;
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

export default function Home() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main>
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading...</div>
          </div>
        </main>
      </div>
    }>
      <HomeContent />
    </Suspense>
  );
}