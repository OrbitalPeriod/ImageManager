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
import { useIsAuthenticated } from '@/lib/auth/hooks';
import { TopNavbar } from '@/components/layout/TopNavbar';
import { SearchSection, type AgeRating } from '@/components/images/SearchSection';
import { ImageGallery, type ImageData } from '@/components/images/ImageGallery';
import { PaginationControls } from '@/components/images/PaginationControls';
import { ErrorPopup } from '@/components/ui/ErrorPopup';
import { useInfiniteScroll } from '@/lib/hooks/useInfiniteScroll';

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
  const [selectedRatings, setSelectedRatings] = useState<AgeRating[]>([0]); // Default to General (0) rating
  const [ownedOnly, setOwnedOnly] = useState(false);

  // Authentication state
  const isAuthenticated = useIsAuthenticated();

  // Pagination state
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(24);

  // Infinite scroll state
  const [isInfiniteScroll, setIsInfiniteScroll] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);

  // Data state
  const [images, setImages] = useState<ImageData[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Infinite scroll detection - only enabled when infinite scroll mode is active
  const { sentinelRef, isIntersecting } = useInfiniteScroll({ enabled: isInfiniteScroll });

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

    const ownedOnlyParam = searchParams.get('ownedOnly');
    if (ownedOnlyParam === 'true') {
      setOwnedOnly(true);
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
    if (ownedOnly && isAuthenticated) {
      params.set('ownedOnly', 'true');
    }

    const newUrl = params.toString() ? `?${params.toString()}` : '/';
    const currentUrl = searchParams.toString() ? `?${searchParams.toString()}` : '/';
    
    if (newUrl !== currentUrl) {
      router.replace(newUrl, { scroll: false });
    }
  }, [debouncedCharacterSearch, debouncedTagSearch, selectedRatings, page, pageSize, ownedOnly, isAuthenticated, router, searchParams]);

  // Fetch images from API
  const fetchImages = useCallback(async (isLoadMore = false) => {
    // Determine if this is a load-more operation based on context
    const isActuallyLoadMore = isLoadMore || (isInfiniteScroll && page > 1);
    
    if (isActuallyLoadMore) {
      setLoadingMore(true);
    } else {
      setLoading(true);
    }
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
        OwnedOnly?: boolean;
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
      if (ownedOnly && isAuthenticated) {
        queryParams.OwnedOnly = true;
      }

      const response = await client.GET('/api/images/search', {
        params: {
          query: queryParams,
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        const isActuallyLoadMore = isLoadMore || (isInfiniteScroll && page > 1);
        if (!isActuallyLoadMore) {
          setImages([]);
          setTotalPages(1);
          setHasMore(false);
        }
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

        // In infinite scroll mode, accumulate images when page > 1
        if (isInfiniteScroll && page > 1) {
          setImages((prev) => {
            // Only accumulate if we have existing images
            if (prev.length > 0) {
              return [...prev, ...sortedImages];
            }
            return sortedImages;
          });
        } else {
          setImages(sortedImages);
        }

        setTotalPages(data.totalPages || 1);
        // Update hasMore: true if current page is less than total pages
        // Use the state 'page' variable since it's what we actually requested
        setHasMore(page < (data.totalPages || 1));
      } else {
        if (!isActuallyLoadMore) {
          setImages([]);
          setTotalPages(1);
        }
        setHasMore(false);
      }
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      if (!isActuallyLoadMore) {
        setImages([]);
        setTotalPages(1);
      }
      setHasMore(false);
    } finally {
      if (isActuallyLoadMore) {
        setLoadingMore(false);
      } else {
        setLoading(false);
      }
    }
  }, [debouncedCharacterSearch, debouncedTagSearch, selectedRatings, page, pageSize, ownedOnly, isAuthenticated, isInfiniteScroll]);

  // Fetch images when dependencies change
  useEffect(() => {
    if (isInitialized.current) {
      fetchImages(false);
    }
  }, [fetchImages]);

  // Reset to page 1 when filters change
  useEffect(() => {
    if (!isInitialized.current) return;
    if (page !== 1) {
      setPage(1);
    }
    // Reset images in infinite scroll mode when filters change
    if (isInfiniteScroll) {
      setImages([]);
      setHasMore(true);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedCharacterSearch, debouncedTagSearch, selectedRatings, ownedOnly, isInfiniteScroll]);

  // Handle infinite scroll: load more when sentinel is visible
  useEffect(() => {
    if (
      isInitialized.current &&
      isInfiniteScroll &&
      isIntersecting &&
      hasMore &&
      !loadingMore &&
      !loading &&
      page > 0
    ) {
      const nextPage = page + 1;
      setPage(nextPage);
    }
  }, [isIntersecting, isInfiniteScroll, hasMore, loadingMore, loading, page]);

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
    if (isInfiniteScroll) {
      setImages([]);
      setHasMore(true);
    }
  };

  // Handle infinite scroll toggle
  const handleInfiniteScrollChange = (enabled: boolean) => {
    setIsInfiniteScroll(enabled);
    if (enabled) {
      // Switching to infinite scroll: keep current images
      setHasMore(page < totalPages);
    } else {
      // Switching back to pagination: reset to page 1 if we're not already there
      if (page !== 1) {
        setPage(1);
      }
    }
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
          isInfiniteScroll={isInfiniteScroll}
          onInfiniteScrollChange={handleInfiniteScrollChange}
          ownedOnly={ownedOnly}
          onOwnedOnlyChange={setOwnedOnly}
          isAuthenticated={isAuthenticated}
        />
        {loading ? (
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading images...</div>
          </div>
        ) : (
          <>
            <ImageGallery images={images} apiBaseUrl={apiBaseUrl} />
            {/* Infinite scroll sentinel - detects when user scrolls near bottom */}
            {/* Always render the sentinel when infinite scroll is enabled, even if empty */}
            {isInfiniteScroll && (
              <div ref={sentinelRef} className="w-full">
                {loadingMore && (
                  <div className="flex items-center justify-center py-8">
                    <div className="text-foreground text-lg">Loading more images...</div>
                  </div>
                )}
                {!hasMore && images.length > 0 && !loadingMore && (
                  <div className="flex items-center justify-center py-8">
                    <div className="text-muted-foreground text-lg">No more images</div>
                  </div>
                )}
                {/* Empty div to ensure sentinel has some height for IntersectionObserver */}
                {!loadingMore && hasMore && <div className="h-4" />}
              </div>
            )}
            {!isInfiniteScroll && (
              <PaginationControls
                currentPage={page}
                totalPages={totalPages}
                pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={handlePageSizeChange}
              />
            )}
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