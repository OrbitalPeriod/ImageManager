/**
 * useAutocomplete Hook
 * Manages autocomplete state and fetches suggestions from API
 */

import { useState, useEffect, useCallback } from 'react';
import { useDebounce } from './useDebounce';

export interface AutocompleteSuggestion {
  id: string;
  label: string;
}

interface UseAutocompleteOptions {
  fetchSuggestions: (query: string) => Promise<AutocompleteSuggestion[]>;
  minChars?: number;
  debounceMs?: number;
}

interface UseAutocompleteReturn {
  suggestions: AutocompleteSuggestion[];
  loading: boolean;
  selectedIndex: number;
  setSelectedIndex: (index: number) => void;
  reset: () => void;
}

/**
 * Extracts the current term being typed from a comma-separated string
 * Example: "char1, char2, cha" -> "cha"
 */
function extractCurrentTerm(value: string): string {
  const lastCommaIndex = value.lastIndexOf(',');
  if (lastCommaIndex === -1) {
    return value.trim();
  }
  return value.slice(lastCommaIndex + 1).trim();
}

export function useAutocomplete(
  value: string,
  options: UseAutocompleteOptions
): UseAutocompleteReturn {
  const { fetchSuggestions, minChars = 2, debounceMs = 300 } = options;
  
  const [suggestions, setSuggestions] = useState<AutocompleteSuggestion[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);

  // Extract current term from the comma-separated value
  const currentTerm = extractCurrentTerm(value);
  
  // Debounce the current term for API calls
  const debouncedTerm = useDebounce(currentTerm, debounceMs);

  // Fetch suggestions when debounced term changes
  useEffect(() => {
    const term = debouncedTerm.trim();
    
    // Don't fetch if term is too short
    if (term.length < minChars) {
      setSuggestions([]);
      setLoading(false);
      setSelectedIndex(-1);
      return;
    }

    let cancelled = false;

    const fetchData = async () => {
      setLoading(true);
      setSelectedIndex(-1);

      try {
        const results = await fetchSuggestions(term);
        if (!cancelled) {
          setSuggestions(results);
          setSelectedIndex(-1);
        }
      } catch (error) {
        if (!cancelled) {
          console.error('Error fetching autocomplete suggestions:', error);
          setSuggestions([]);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    fetchData();

    return () => {
      cancelled = true;
    };
  }, [debouncedTerm, fetchSuggestions, minChars]);

  // Reset suggestions when value changes significantly (e.g., user selects a suggestion)
  const reset = useCallback(() => {
    setSuggestions([]);
    setSelectedIndex(-1);
    setLoading(false);
  }, []);

  return {
    suggestions,
    loading,
    selectedIndex,
    setSelectedIndex,
    reset,
  };
}
