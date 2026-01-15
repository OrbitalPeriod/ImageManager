/**
 * Image Gallery Component
 * Based on the example ImageGrid design
 */

'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { cn } from '@/lib/utils/cn';

export interface ImageData {
  id: string;
  rating: number;
  title?: string;
  artist?: string;
  likes?: number;
  views?: number;
}

interface ImageGalleryProps {
  images: ImageData[];
  apiBaseUrl?: string;
}

const HeartIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
  </svg>
);

const EyeIcon = () => (
  <svg className="h-3 w-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
  </svg>
);

const DownloadIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
  </svg>
);

const getRatingBadge = (rating: number): { label: string; className: string } | null => {
  switch (rating) {
    case 0:
      return { label: 'G', className: 'bg-safe/20 text-safe' }; // General
    case 1:
      return { label: 'S', className: 'bg-sensitive/20 text-sensitive' }; // Sensitive
    case 2:
      return { label: 'Q', className: 'bg-warning/20 text-warning' }; // Questionable
    case 3:
      return { label: 'E', className: 'bg-destructive/20 text-destructive' }; // Explicit
    default:
      return null;
  }
};

export function ImageGallery({ images, apiBaseUrl = '' }: ImageGalleryProps) {
  const [imageErrors, setImageErrors] = useState<Set<string>>(new Set());
  const [downloadingImages, setDownloadingImages] = useState<Set<string>>(new Set());

  const handleImageError = (imageId: string) => {
    setImageErrors((prev) => new Set(prev).add(imageId));
  };

  // Download original image
  const handleDownload = async (e: React.MouseEvent, imageId: string) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!imageId || !apiBaseUrl) return;

    try {
      setDownloadingImages((prev) => new Set(prev).add(imageId));
      const downloadUrl = `${apiBaseUrl}/api/images/${imageId}/original`;
      
      // Fetch the image with credentials to include auth cookies
      const response = await fetch(downloadUrl, {
        credentials: 'include',
        method: 'GET',
      });

      if (!response.ok) {
        throw new Error(`Failed to download image: ${response.statusText}`);
      }

      // Get the image as a blob
      const blob = await response.blob();
      
      // Determine file extension from content type or default to jpg
      const contentType = response.headers.get('content-type') || 'image/jpeg';
      const extension = contentType.includes('png') ? 'png' : 
                       contentType.includes('webp') ? 'webp' : 
                       contentType.includes('gif') ? 'gif' : 'jpg';
      
      // Create a download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `image-${imageId.substring(0, 8)}.${extension}`;
      document.body.appendChild(link);
      link.click();
      
      // Clean up
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Failed to download image:', err);
      // Optionally show a toast or error message here
    } finally {
      setDownloadingImages((prev) => {
        const next = new Set(prev);
        next.delete(imageId);
        return next;
      });
    }
  };

  if (images.length === 0) {
    return (
      <section className="w-full py-8">
        <div className="container mx-auto px-4">
          <div className="flex items-center justify-center py-20">
            <p className="text-foreground text-lg">No images found</p>
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="w-full py-8">
      <div className="container mx-auto px-4">
        <div className="columns-2 md:columns-3 lg:columns-4 xl:columns-5 gap-4 space-y-4">
          {images.map((image) => {
            const badge = getRatingBadge(image.rating);
            const hasError = imageErrors.has(image.id);
            const thumbnailUrl = `${apiBaseUrl}/api/images/${image.id}/thumb`;

            return (
              <Link
                key={image.id}
                href={`/image/${image.id}`}
                className="break-inside-avoid group relative overflow-hidden rounded-lg bg-card border border-border/50 hover:border-primary/50 transition-all duration-300 block"
              >
                {/* Image */}
                <div className="relative overflow-hidden">
                  {!hasError ? (
                    <img
                      src={thumbnailUrl}
                      alt={image.title || `Image ${image.id}`}
                      className="w-full object-cover transition-transform duration-500 group-hover:scale-105"
                      loading="lazy"
                      onError={() => handleImageError(image.id)}
                    />
                  ) : (
                    <div className="w-full aspect-[2/3] flex items-center justify-center bg-card">
                      <p className="text-muted-foreground text-sm">Failed to load</p>
                    </div>
                  )}

                  {/* Overlay on hover */}
                  <div className="absolute inset-0 bg-gradient-to-t from-background/90 via-background/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                    {/* Quick Actions */}
                    <div className="absolute top-3 right-3 flex gap-2">
                      <button 
                        className="p-2 rounded-full glass hover:bg-primary/20 transition-colors"
                        onClick={(e) => e.preventDefault()}
                      >
                        <HeartIcon />
                      </button>
                      <button 
                        className="p-2 rounded-full glass hover:bg-primary/20 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        onClick={(e) => handleDownload(e, image.id)}
                        disabled={downloadingImages.has(image.id)}
                        title={downloadingImages.has(image.id) ? 'Downloading...' : 'Download original image'}
                      >
                        <DownloadIcon />
                      </button>
                    </div>

                    {/* Info */}
                    {(image.title || image.artist || image.likes || image.views) && (
                      <div className="absolute bottom-0 left-0 right-0 p-4 pointer-events-none">
                        {image.title && (
                          <h3 className="font-medium text-foreground truncate">{image.title}</h3>
                        )}
                        {image.artist && (
                          <p className="text-sm text-muted-foreground">by {image.artist}</p>
                        )}
                        {(image.likes || image.views) && (
                          <div className="flex items-center gap-4 mt-2 text-xs text-muted-foreground">
                            {image.likes !== undefined && (
                              <span className="flex items-center gap-1">
                                <HeartIcon /> {image.likes}
                              </span>
                            )}
                            {image.views !== undefined && (
                              <span className="flex items-center gap-1">
                                <EyeIcon /> {image.views}
                              </span>
                            )}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>

                {/* Rating Badge */}
                {badge && (
                  <div
                    className={cn(
                      'absolute top-3 left-3 px-2 py-0.5 rounded text-xs font-medium uppercase tracking-wider pointer-events-none',
                      badge.className
                    )}
                  >
                    {badge.label}
                  </div>
                )}
              </Link>
            );
          })}
        </div>
      </div>
    </section>
  );
}