/**
 * useInfiniteScroll Hook
 * Detects when the user scrolls near the bottom of the page using IntersectionObserver
 */

import { useEffect, useRef, useState, useCallback } from 'react';

interface UseInfiniteScrollOptions {
  root?: Element | null;
  rootMargin?: string;
  threshold?: number;
  enabled?: boolean;
}

export function useInfiniteScroll(options: UseInfiniteScrollOptions = {}) {
  const [isIntersecting, setIsIntersecting] = useState(false);
  const observerRef = useRef<IntersectionObserver | null>(null);
  const elementRef = useRef<HTMLDivElement | null>(null);

  const { root = null, rootMargin = '100px', threshold = 0, enabled = true } = options;

  // Callback ref to set up observer when element is mounted
  const sentinelRef = useCallback(
    (node: HTMLDivElement | null) => {
      // Clean up existing observer
      if (observerRef.current) {
        observerRef.current.disconnect();
        observerRef.current = null;
      }

      elementRef.current = node;

      // If disabled or no node, don't observe
      if (!enabled || !node) {
        setIsIntersecting(false);
        return;
      }

      // Create and observe
      const observer = new IntersectionObserver(
        ([entry]) => {
          setIsIntersecting(entry.isIntersecting);
        },
        {
          root,
          rootMargin,
          threshold,
        }
      );

      observerRef.current = observer;
      observer.observe(node);
    },
    [enabled, root, rootMargin, threshold]
  );

  // Cleanup on unmount or when enabled changes
  useEffect(() => {
    return () => {
      if (observerRef.current) {
        observerRef.current.disconnect();
        observerRef.current = null;
      }
    };
  }, []);

  // Update intersection state when disabled
  useEffect(() => {
    if (!enabled) {
      setIsIntersecting(false);
    }
  }, [enabled]);

  return { sentinelRef, isIntersecting };
}
