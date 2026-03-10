/**
 * Search and Filter Section Component
 * Based on the example SearchSection design
 */

'use client';

import React, { useCallback, useEffect, useState } from 'react';
import { cn } from '@/lib/utils/cn';
import { AutocompleteInput } from '@/components/ui/AutocompleteInput';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleApiResponseError, handleError } from '@/lib/errors/handlers';
import type { AutocompleteSuggestion } from '@/lib/hooks/useAutocomplete';
import type { components } from '@/lib/api/client';

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
  selectedFolders?: string[];
  onFoldersChange?: (folderIds: string[]) => void;
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

const FolderIcon = () => (
  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
  </svg>
);

type FolderDto = components['schemas']['FolderDto'];

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
  selectedFolders = [],
  onFoldersChange,
}: SearchSectionProps) {
  const isRatingSelected = (rating: AgeRating) => selectedRatings.includes(rating);
  const [folders, setFolders] = useState<FolderDto[]>([]);
  const [foldersLoading, setFoldersLoading] = useState(false);

  // Fetch folders for authenticated users
  useEffect(() => {
    if (!isAuthenticated || !onFoldersChange) {
      setFolders([]);
      return;
    }

    const fetchFolders = async () => {
      setFoldersLoading(true);
      try {
        const client = getClientApiClient();
        const response = await client.GET('/api/folders');

        if (response.error) {
          // Silently fail - folders are optional
          setFolders([]);
          return;
        }

        const data = response.data as FolderDto[] | undefined;
        if (data) {
          setFolders(data);
        } else {
          setFolders([]);
        }
      } catch (err) {
        // Silently fail - folders are optional
        setFolders([]);
      } finally {
        setFoldersLoading(false);
      }
    };

    fetchFolders();
  }, [isAuthenticated, onFoldersChange]);

  const handleFolderToggle = (folderId: string) => {
    if (!onFoldersChange || !folderId) return;

    const isSelected = selectedFolders.includes(folderId);
    if (isSelected) {
      onFoldersChange(selectedFolders.filter((id) => id !== folderId));
    } else {
      onFoldersChange([...selectedFolders, folderId]);
    }
  };

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

        {/* Folder Filter - Only for authenticated users */}
        {isAuthenticated && onFoldersChange && folders.length > 0 && (
          <div className="flex flex-col items-center gap-3">
            <span className="text-sm text-muted-foreground font-medium">Filter by Folders:</span>
            <div className="flex flex-wrap items-center justify-center gap-2 max-w-2xl">
              {folders.map((folder) => {
                const isSelected = folder.id ? selectedFolders.includes(folder.id) : false;
                const isLiked = folder.name === 'Liked';
                return (
                  <button
                    key={folder.id}
                    onClick={() => folder.id && handleFolderToggle(folder.id)}
                    className={cn(
                      'inline-flex items-center gap-2 px-3 py-2 rounded-md text-sm font-medium transition-colors',
                      'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
                      isSelected
                        ? 'bg-primary text-primary-foreground'
                        : 'bg-muted text-muted-foreground hover:bg-muted/80',
                      isLiked && isSelected && 'bg-primary/90'
                    )}
                  >
                    <FolderIcon />
                    {folder.name || 'Unnamed Folder'}
                    {isSelected && (
                      <svg className="h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                      </svg>
                    )}
                  </button>
                );
              })}
            </div>
            {selectedFolders.length > 0 && (
              <button
                onClick={() => onFoldersChange([])}
                className="text-xs text-muted-foreground hover:text-foreground underline"
              >
                Clear folder filters
              </button>
            )}
          </div>
        )}

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