/**
 * Search and Filter Section Component
 * Based on the example SearchSection design
 */

'use client';

import React from 'react';
import { cn } from '@/lib/utils/cn';

export type AgeRating = 0 | 1 | 2 | 3; // General, Sensitive, Questionable, Explicit

interface SearchSectionProps {
  characterSearch: string;
  tagSearch: string;
  selectedRatings: AgeRating[];
  onCharacterSearchChange: (value: string) => void;
  onTagSearchChange: (value: string) => void;
  onRatingChange: (rating: AgeRating) => void;
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
}: SearchSectionProps) {
  const isRatingSelected = (rating: AgeRating) => selectedRatings.includes(rating);

  return (
    <section className="w-full py-8">
      <div className="container mx-auto px-4 space-y-6">
        {/* Search Bars */}
        <div className="grid md:grid-cols-2 gap-4">
          {/* Character Search */}
          <div className="relative group">
            <div className="absolute inset-0 bg-primary/10 rounded-lg blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
            <div className="relative glass rounded-lg p-1 group-focus-within:ring-2 group-focus-within:ring-primary/50 transition-all">
              <div className="flex items-center gap-3 px-4 py-3">
                <UsersIcon className="text-primary shrink-0" />
                <input
                  type="text"
                  placeholder="Search characters..."
                  value={characterSearch}
                  onChange={(e) => onCharacterSearchChange(e.target.value)}
                  className="flex-1 border-0 bg-transparent p-0 h-auto text-base placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-0"
                />
              </div>
            </div>
          </div>

          {/* Tag Search */}
          <div className="relative group">
            <div className="absolute inset-0 bg-primary/10 rounded-lg blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
            <div className="relative glass rounded-lg p-1 group-focus-within:ring-2 group-focus-within:ring-primary/50 transition-all">
              <div className="flex items-center gap-3 px-4 py-3">
                <TagsIcon className="text-primary shrink-0" />
                <input
                  type="text"
                  placeholder="Search tags..."
                  value={tagSearch}
                  onChange={(e) => onTagSearchChange(e.target.value)}
                  className="flex-1 border-0 bg-transparent p-0 h-auto text-base placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-0"
                />
              </div>
            </div>
          </div>
        </div>

        {/* Age Rating Selector */}
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
      </div>
    </section>
  );
}