/**
 * Pagination Controls Component
 * Based on the example Pagination design
 */

'use client';

import React from 'react';
import { cn } from '@/lib/utils/cn';

interface PaginationControlsProps {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

const ChevronLeftIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
  </svg>
);

const ChevronRightIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
  </svg>
);

const ChevronsLeftIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
  </svg>
);

const ChevronsRightIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 5l7 7-7 7M5 5l7 7-7 7" />
  </svg>
);

const PAGE_SIZE_OPTIONS = [12, 24, 48, 96];

export function PaginationControls({
  currentPage,
  totalPages,
  pageSize,
  onPageChange,
  onPageSizeChange,
}: PaginationControlsProps) {
  const goToPage = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      onPageChange(page);
    }
  };

  const getVisiblePages = () => {
    const pages: number[] = [];
    const start = Math.max(1, currentPage - 2);
    const end = Math.min(totalPages, currentPage + 2);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  };

  if (totalPages === 0) {
    return null;
  }

  return (
    <section className="w-full py-8 border-t border-border/50">
      <div className="container mx-auto px-4">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          {/* Items per page */}
          <div className="flex items-center gap-3">
            <span className="text-sm text-muted-foreground">Items per page:</span>
            <select
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
              className="w-20 glass border-border/50 focus:ring-primary/50 rounded-md px-3 py-2 text-sm bg-popover text-popover-foreground focus:outline-none focus:ring-2"
            >
              {PAGE_SIZE_OPTIONS.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>

          {/* Page Navigation */}
          <div className="flex items-center gap-1">
            <button
              onClick={() => goToPage(1)}
              disabled={currentPage === 1}
              className={cn(
                'inline-flex items-center justify-center h-10 w-10 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-30'
              )}
            >
              <ChevronsLeftIcon />
            </button>
            <button
              onClick={() => goToPage(currentPage - 1)}
              disabled={currentPage === 1}
              className={cn(
                'inline-flex items-center justify-center h-10 w-10 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-30'
              )}
            >
              <ChevronLeftIcon />
            </button>

            <div className="flex items-center gap-1 mx-2">
              {currentPage > 3 && (
                <>
                  <button
                    onClick={() => goToPage(1)}
                    className="inline-flex items-center justify-center h-8 w-8 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  >
                    1
                  </button>
                  {currentPage > 4 && (
                    <span className="text-muted-foreground px-1">...</span>
                  )}
                </>
              )}

              {getVisiblePages().map((page) => (
                <button
                  key={page}
                  onClick={() => goToPage(page)}
                  className={cn(
                    'inline-flex items-center justify-center h-8 w-8 rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
                    page === currentPage
                      ? 'bg-primary text-primary-foreground glow-primary-sm'
                      : 'hover:bg-primary/10 hover:text-primary'
                  )}
                >
                  {page}
                </button>
              ))}

              {currentPage < totalPages - 2 && (
                <>
                  {currentPage < totalPages - 3 && (
                    <span className="text-muted-foreground px-1">...</span>
                  )}
                  <button
                    onClick={() => goToPage(totalPages)}
                    className="inline-flex items-center justify-center h-8 w-8 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                  >
                    {totalPages}
                  </button>
                </>
              )}
            </div>

            <button
              onClick={() => goToPage(currentPage + 1)}
              disabled={currentPage === totalPages}
              className={cn(
                'inline-flex items-center justify-center h-10 w-10 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-30'
              )}
            >
              <ChevronRightIcon />
            </button>
            <button
              onClick={() => goToPage(totalPages)}
              disabled={currentPage === totalPages}
              className={cn(
                'inline-flex items-center justify-center h-10 w-10 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-30'
              )}
            >
              <ChevronsRightIcon />
            </button>
          </div>

          {/* Page info */}
          <div className="text-sm text-muted-foreground">
            Page <span className="text-foreground font-medium">{currentPage}</span> of{' '}
            <span className="text-foreground font-medium">{totalPages}</span>
          </div>
        </div>
      </div>
    </section>
  );
}