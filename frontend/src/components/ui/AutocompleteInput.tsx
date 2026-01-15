/**
 * AutocompleteInput Component
 * A reusable input component with dropdown suggestions
 */

'use client';

import React, { useRef, useEffect, useState, KeyboardEvent } from 'react';
import { useAutocomplete, type AutocompleteSuggestion } from '@/lib/hooks/useAutocomplete';
import { cn } from '@/lib/utils/cn';

interface AutocompleteInputProps {
  value: string;
  onChange: (value: string) => void;
  onFetchSuggestions: (query: string) => Promise<AutocompleteSuggestion[]>;
  placeholder: string;
  icon?: React.ReactNode;
  minChars?: number;
}

/**
 * Inserts a suggestion into a comma-separated string, replacing the current term
 * Example: insertSuggestion("char1, char2, cha", "character") -> "char1, char2, character"
 */
function insertSuggestion(value: string, suggestion: string): string {
  const lastCommaIndex = value.lastIndexOf(',');
  if (lastCommaIndex === -1) {
    // No comma, replace entire value
    return suggestion;
  }
  // Replace text after last comma
  const beforeComma = value.slice(0, lastCommaIndex + 1).trim();
  return `${beforeComma} ${suggestion}`;
}

export function AutocompleteInput({
  value,
  onChange,
  onFetchSuggestions,
  placeholder,
  icon,
  minChars = 2,
}: AutocompleteInputProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const [isFocused, setIsFocused] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);

  const {
    suggestions,
    loading,
    selectedIndex,
    setSelectedIndex,
    reset,
  } = useAutocomplete(value, {
    fetchSuggestions: onFetchSuggestions,
    minChars,
    debounceMs: 300,
  });

  // Show dropdown when there are suggestions and input is focused
  useEffect(() => {
    setShowDropdown(isFocused && (suggestions.length > 0 || loading));
  }, [isFocused, suggestions.length, loading]);

  // Handle click outside to close dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsFocused(false);
        setShowDropdown(false);
        reset();
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [reset]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
    setSelectedIndex(-1);
  };

  const handleInputFocus = () => {
    setIsFocused(true);
  };

  const handleInputBlur = () => {
    // Delay blur to allow click on suggestions
    setTimeout(() => {
      setIsFocused(false);
      setShowDropdown(false);
    }, 200);
  };

  const handleSuggestionClick = (suggestion: AutocompleteSuggestion) => {
    const newValue = insertSuggestion(value, suggestion.label);
    onChange(newValue);
    reset();
    inputRef.current?.focus();
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (!showDropdown || suggestions.length === 0) {
      if (e.key === 'Enter') {
        e.preventDefault();
      }
      return;
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex(selectedIndex < suggestions.length - 1 ? selectedIndex + 1 : selectedIndex);
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex(selectedIndex > 0 ? selectedIndex - 1 : -1);
        break;
      case 'Enter':
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < suggestions.length) {
          handleSuggestionClick(suggestions[selectedIndex]);
        }
        break;
      case 'Escape':
        e.preventDefault();
        setShowDropdown(false);
        reset();
        inputRef.current?.blur();
        break;
    }
  };

  return (
    <div ref={containerRef} className="relative group">
      <div className="absolute inset-0 bg-primary/10 rounded-lg blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
      <div className="relative glass rounded-lg p-1 group-focus-within:ring-2 group-focus-within:ring-primary/50 transition-all">
        <div className="flex items-center gap-3 px-4 py-3">
          {icon && <div className="text-primary shrink-0">{icon}</div>}
          <input
            ref={inputRef}
            type="text"
            placeholder={placeholder}
            value={value}
            onChange={handleInputChange}
            onFocus={handleInputFocus}
            onBlur={handleInputBlur}
            onKeyDown={handleKeyDown}
            className="flex-1 border-0 bg-transparent p-0 h-auto text-base placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-0"
          />
        </div>
      </div>

      {/* Dropdown */}
      {showDropdown && (
        <div className="absolute z-50 w-full mt-1 glass rounded-lg shadow-lg border border-border/50 max-h-60 overflow-auto">
          {loading ? (
            <div className="px-4 py-3 text-sm text-muted-foreground">
              Loading suggestions...
            </div>
          ) : suggestions.length === 0 ? (
            <div className="px-4 py-3 text-sm text-muted-foreground">
              No suggestions found
            </div>
          ) : (
            <ul className="py-1">
              {suggestions.map((suggestion, index) => (
                <li
                  key={`${suggestion.id}-${index}`}
                  onClick={() => handleSuggestionClick(suggestion)}
                  className={cn(
                    'px-4 py-2 text-sm cursor-pointer transition-colors',
                    index === selectedIndex
                      ? 'bg-primary/20 text-primary'
                      : 'hover:bg-muted/50 text-foreground'
                  )}
                >
                  {suggestion.label}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
