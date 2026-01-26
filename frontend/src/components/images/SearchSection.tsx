/**
 * Search and Filter Section Component
 * Based on the example SearchSection design
 */

'use client';

import React, { useCallback } from 'react';
import { cn } from '@/lib/utils/cn';
import { AutocompleteInput } from '@/components/ui/AutocompleteInput';
import { getClientApiClient } from '@/lib/api/client-client';
import type { AutocompleteSuggestion } from '@/lib/hooks/useAutocomplete';

export type AgeRating = 0 | 1 | 2 | 3; // General, Sensitive, Questionable, Explicit

interface SearchSectionProps {
  characterSearch: string;
  tagSearch: string;
  selectedRatings: AgeRating[];
  onCharacterSearchChange: (value: string) => void;
  onTagSearchChange: (value: string) => void;
  onRatingChange: (rating: AgeRating) => void;
  isInfiniteScroll?: boolean;
  onInfiniteScrollChange?: (enabled: boolean) => void;
  ownedOnly?: boolean;
  onOwnedOnlyChange?: (enabled: boolean) => void;
  isAuthenticated?: boolean;
}

const UsersIcon = () => (
  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
  </svg>
);

const TagsIcon = () => (
  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
  </svg>
);

const AGE_RATINGS: { value: AgeRating; label: string; bgColor: string; textColor: string }[] = [
  { value: 0, label: 'General', bgColor: 'bg-safe', textColor: 'text-safe-foreground' },
  { value: 1, label: 'Sensitive', bgColor: 'bg-sensitive', textColor: 'text-sensitive-foreground' },
  { value: 2, label: 'Questionable', bgColor: 'bg-warning', textColor: 'text-warning-foreground' },
  { value: 3, label: 'Explicit', bgColor: 'bg-destructive', textColor: 'text-destructive-foreground' },
];

export function SearchSection({
  characterSearch,
  tagSearch,
  selectedRatings,
  onCharacterSearchChange,
  onTagSearchChange,
  onRatingChange,
  isInfiniteScroll = false,
  onInfiniteScrollChange,
  ownedOnly = false,
  onOwnedOnlyChange,
  isAuthenticated = false,
}: SearchSectionProps) {
  const isRatingSelected = (rating: AgeRating) => selectedRatings.includes(rating);

  // Fetch character suggestions
  const fetchCharacterSuggestions = useCallback(async (query: string): Promise<AutocompleteSuggestion[]> => {
    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/characters/search', {
        params: {
          query: {
            q: query,
            page: 1,
            pageSize: 10,
          },
        },
      });

      if (response.error || !response.data) {
        return [];
      }

      const characters = response.data.data || [];
      return characters
        .filter((char) => char.characterName)
        .map((char) => ({
          id: char.tagId || char.characterName || '',
          label: char.characterName || '',
        }));
    } catch (error) {
      console.error('Error fetching character suggestions:', error);
      return [];
    }
  }, []);

  // Fetch tag suggestions
  const fetchTagSuggestions = useCallback(async (query: string): Promise<AutocompleteSuggestion[]> => {
    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/tags/search', {
        params: {
          query: {
            q: query,
            page: 1,
            pageSize: 10,
          },
        },
      });

      if (response.error || !response.data) {
        return [];
      }

      const tags = response.data.data || [];
      return tags
        .filter((tag) => tag.tagName)
        .map((tag) => ({
          id: tag.tagId || tag.tagName || '',
          label: tag.tagName || '',
        }));
    } catch (error) {
      console.error('Error fetching tag suggestions:', error);
      return [];
    }
  }, []);

  return (
    <section className="w-full py-8">
      <div className="container mx-auto px-4 space-y-6">
        {/* Search Bars */}
        <div className="grid md:grid-cols-2 gap-4">
          {/* Character Search */}
          <AutocompleteInput
            value={characterSearch}
            onChange={onCharacterSearchChange}
            onFetchSuggestions={fetchCharacterSuggestions}
            placeholder="Search characters..."
            icon={<UsersIcon />}
            minChars={2}
          />

          {/* Tag Search */}
          <AutocompleteInput
            value={tagSearch}
            onChange={onTagSearchChange}
            onFetchSuggestions={fetchTagSuggestions}
            placeholder="Search tags..."
            icon={<TagsIcon />}
            minChars={2}
          />
        </div>

        {/* Age Rating Selector and Infinite Scroll Toggle */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
          <span className="text-sm text-muted-foreground font-medium">Age Rating:</span>
          <div className="flex items-center justify-center gap-1 glass rounded-lg p-1">
            {AGE_RATINGS.map(({ value, label, bgColor, textColor }) => {
              const isSelected = isRatingSelected(value);
              return (
                <button
                  key={value}
                  onClick={() => onRatingChange(value)}
                  className={cn(
                    'inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 px-4 py-2',
                    isSelected
                      ? `${bgColor} ${textColor}`
                      : 'bg-transparent hover:bg-muted hover:text-muted-foreground text-foreground'
                  )}
                >
                  {label}
                </button>
              );
            })}
          </div>
        </div>

        {/* Ownership Filter - Only for authenticated users */}
        {isAuthenticated && onOwnedOnlyChange && (
          <div className="flex items-center justify-center gap-3">
            <span className="text-sm text-muted-foreground">All Images</span>
            <button
              onClick={() => onOwnedOnlyChange(!ownedOnly)}
              className={cn(
                'relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background',
                ownedOnly ? 'bg-primary' : 'bg-muted'
              )}
              role="switch"
              aria-checked={ownedOnly}
            >
              <span
                className={cn(
                  'inline-block h-4 w-4 transform rounded-full bg-background transition-transform',
                  ownedOnly ? 'translate-x-6' : 'translate-x-1'
                )}
              />
            </button>
            <span className="text-sm text-muted-foreground">My Images</span>
          </div>
        )}

        {/* Infinite Scroll Toggle */}
        {onInfiniteScrollChange && (
          <div className="flex items-center justify-center gap-3">
            <span className="text-sm text-muted-foreground">Pagination</span>
            <button
              onClick={() => onInfiniteScrollChange(!isInfiniteScroll)}
              className={cn(
                'relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background',
                isInfiniteScroll ? 'bg-primary' : 'bg-muted'
              )}
              role="switch"
              aria-checked={isInfiniteScroll}
            >
              <span
                className={cn(
                  'inline-block h-4 w-4 transform rounded-full bg-background transition-transform',
                  isInfiniteScroll ? 'translate-x-6' : 'translate-x-1'
                )}
              />
            </button>
            <span className="text-sm text-muted-foreground">Infinite Scroll</span>
          </div>
        )}
      </div>
    </section>
  );
}