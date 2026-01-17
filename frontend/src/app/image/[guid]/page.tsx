/**
 * Image Detail Page
 * Displays detailed information about a single image with normal/original quality switching
 */

'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleApiResponseError, handleError, getErrorMessage } from '@/lib/errors/handlers';
import { useShareToken } from '@/lib/sharertoken/hooks';
import { useAuth } from '@/lib/auth/context';
import { TopNavbar } from '@/components/layout/TopNavbar';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ErrorPopup } from '@/components/ui/ErrorPopup';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { cn } from '@/lib/utils/cn';

// Type definitions
interface ImageDataResponse {
  id: string;
  tags?: string[] | null;
  characters?: string[] | null;
  rating: number; // 0-3: General, Sensitive, Questionable, Explicit
  ownerIds?: string[] | null;
  storedAt?: string;
}

// Icon components
const ArrowLeftIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
  </svg>
);

const HeartIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
  </svg>
);

const DownloadIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
  </svg>
);

const ShareIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
  </svg>
);

const FlagIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
  </svg>
);

const ExternalLinkIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
  </svg>
);

const TrashIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
  </svg>
);

const OwnerIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
  </svg>
);

// Rating conversion
const getRatingLabel = (rating: number): string => {
  switch (rating) {
    case 0:
      return 'G'; // General
    case 1:
      return 'S'; // Sensitive
    case 2:
      return 'Q'; // Questionable
    case 3:
      return 'E'; // Explicit
    default:
      return 'G';
  }
};

const getRatingBadge = (rating: number): { label: string; className: string } => {
  switch (rating) {
    case 0:
      return { label: 'G', className: 'bg-safe/20 text-safe' };
    case 1:
      return { label: 'S', className: 'bg-sensitive/20 text-sensitive' };
    case 2:
      return { label: 'Q', className: 'bg-warning/20 text-warning' };
    case 3:
      return { label: 'E', className: 'bg-destructive/20 text-destructive' };
    default:
      return { label: 'G', className: 'bg-safe/20 text-safe' };
  }
};

const getAgeRatingColor = (rating: number): string => {
  switch (rating) {
    case 0:
      return 'bg-emerald-500/20 text-emerald-400 border-emerald-500/50';
    case 1:
      return 'bg-blue-500/20 text-blue-400 border-blue-500/50';
    case 2:
      return 'bg-amber-500/20 text-amber-400 border-amber-500/50';
    case 3:
      return 'bg-rose-500/20 text-rose-400 border-rose-500/50';
    default:
      return 'bg-muted text-muted-foreground';
  }
};

const getRatingFullLabel = (rating: number): string => {
  switch (rating) {
    case 0:
      return 'General';
    case 1:
      return 'Sensitive';
    case 2:
      return 'Questionable';
    case 3:
      return 'Explicit';
    default:
      return 'General';
  }
};

export default function ImageDetailPage() {
  const params = useParams();
  const router = useRouter();
  const guid = params?.guid as string;
  const { token } = useShareToken();
  const { user } = useAuth();

  const [imageData, setImageData] = useState<ImageDataResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [useOriginalQuality, setUseOriginalQuality] = useState(false);
  const [imageLoading, setImageLoading] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);

  const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || '';

  // Check if current user owns the image
  const isOwner = user?.id && imageData?.ownerIds?.includes(user.id);

  // Fetch image metadata
  const fetchImageData = useCallback(async () => {
    if (!guid) return;

    setLoading(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/images/{imageId}/data', {
        params: {
          path: {
            imageId: guid,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setImageData(null);
        return;
      }

      const data = response.data as ImageDataResponse | undefined;
      if (data) {
        setImageData(data);
      } else {
        setError('Image not found');
        setImageData(null);
      }
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      setImageData(null);
    } finally {
      setLoading(false);
    }
  }, [guid]);

  useEffect(() => {
    fetchImageData();
  }, [fetchImageData]);

  // Handle image quality switch
  const handleImageClick = () => {
    if (!useOriginalQuality) {
      setImageLoading(true);
      setUseOriginalQuality(true);
    }
  };

  const handleImageLoad = () => {
    setImageLoading(false);
  };

  const handleImageError = () => {
    setImageLoading(false);
    // Fallback to normal quality if original fails
    if (useOriginalQuality) {
      setUseOriginalQuality(false);
      setError('Failed to load original quality image');
    }
  };

  // Get image URL based on quality setting
  const getImageUrl = (): string => {
    if (!guid) return '';
    const qualityPath = useOriginalQuality ? 'original' : '';
    let url = `${apiBaseUrl}/api/images/${guid}${qualityPath ? '/' + qualityPath : ''}`;
    if (token) {
      url += `${url.includes('?') ? '&' : '?'}token=${encodeURIComponent(token)}`;
    }
    return url;
  };

  // Download original image
  const handleDownload = async (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!guid || !apiBaseUrl) return;

    try {
      setImageLoading(true);
      let downloadUrl = `${apiBaseUrl}/api/images/${guid}/original`;
      if (token) {
        downloadUrl += `?token=${encodeURIComponent(token)}`;
      }
      
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
      link.download = `image-${guid.substring(0, 8)}.${extension}`;
      document.body.appendChild(link);
      link.click();
      
      // Clean up
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (err) {
      const appError = handleError(err);
      setError(`Failed to download image: ${getErrorMessage(appError)}`);
    } finally {
      setImageLoading(false);
    }
  };

  // Delete image
  const handleDelete = async () => {
    if (!guid) return;

    setDeleting(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.DELETE('/api/images/{imageId}', {
        params: {
          path: {
            imageId: guid,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setDeleting(false);
        setDeleteDialogOpen(false);
        return;
      }

      // Successfully deleted, redirect to gallery
      router.push('/');
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      setDeleting(false);
      setDeleteDialogOpen(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main className="container mx-auto px-4 py-6">
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading image...</div>
          </div>
        </main>
      </div>
    );
  }

  if (error && !imageData) {
    return (
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main className="container mx-auto px-4 py-6">
          <div className="flex flex-col items-center justify-center py-20 gap-4">
            <div className="text-foreground text-lg">{error}</div>
            <Link href="/">
              <Button variant="outline">Back to Gallery</Button>
            </Link>
          </div>
        </main>
        <ErrorPopup message={error} onClose={() => setError(null)} />
      </div>
    );
  }

  if (!imageData) {
    return (
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main className="container mx-auto px-4 py-6">
          <div className="flex flex-col items-center justify-center py-20 gap-4">
            <div className="text-foreground text-lg">Image not found</div>
            <Link href="/">
              <Button variant="outline">Back to Gallery</Button>
            </Link>
          </div>
        </main>
      </div>
    );
  }

  const ratingBadge = getRatingBadge(imageData.rating);
  const ratingColor = getAgeRatingColor(imageData.rating);
  const ratingLabel = getRatingFullLabel(imageData.rating);

  return (
    <div className="min-h-screen bg-background">
      <TopNavbar />
      
      <main className="container mx-auto px-4 py-6">
        {/* Back Button */}
        <Link href="/">
          <Button variant="ghost" className="mb-4 gap-2 hover:bg-primary/10 hover:text-primary">
            <ArrowLeftIcon />
            Back to Gallery
          </Button>
        </Link>

        <div className="grid grid-cols-1 lg:grid-cols-[320px_1fr] gap-6">
          {/* Left Sidebar - Metadata */}
          <div className="space-y-4">
            {/* Age Rating */}
            <Card className="glass border-border/50">
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground uppercase tracking-wider">
                  Age Rating
                </CardTitle>
              </CardHeader>
              <CardContent>
                <Badge 
                  variant="outline" 
                  className={cn("text-base px-4 py-2", ratingColor)}
                >
                  {ratingLabel}
                </Badge>
              </CardContent>
            </Card>

            {/* Characters */}
            {imageData.characters && imageData.characters.length > 0 && (
              <Card className="glass border-border/50">
                <CardHeader className="pb-3">
                  <CardTitle className="text-sm font-medium text-muted-foreground uppercase tracking-wider">
                    Characters ({imageData.characters.length})
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex flex-wrap gap-2">
                    {imageData.characters.map((character) => (
                      <Link key={character} href={`/?characters=${encodeURIComponent(character)}`}>
                        <Badge 
                          variant="secondary" 
                          className="cursor-pointer hover:bg-primary/20 hover:text-primary transition-colors bg-secondary/50"
                        >
                          {character}
                        </Badge>
                      </Link>
                    ))}
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Tags */}
            {imageData.tags && imageData.tags.length > 0 && (
              <Card className="glass border-border/50">
                <CardHeader className="pb-3">
                  <CardTitle className="text-sm font-medium text-muted-foreground uppercase tracking-wider">
                    Tags ({imageData.tags.length})
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex flex-wrap gap-2">
                    {imageData.tags.map((tag) => (
                      <Link key={tag} href={`/?tags=${encodeURIComponent(tag)}`}>
                        <Badge 
                          variant="outline" 
                          className="cursor-pointer hover:bg-primary/20 hover:text-primary hover:border-primary/50 transition-colors border-border/50"
                        >
                          #{tag}
                        </Badge>
                      </Link>
                    ))}
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Image Info */}
            <Card className="glass border-border/50">
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground uppercase tracking-wider">
                  Image Info
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">ID</span>
                  <span className="text-foreground font-mono text-xs">{imageData.id.substring(0, 8)}...</span>
                </div>
                {imageData.storedAt && (
                  <>
                    <Separator className="bg-border/50" />
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Stored At</span>
                      <span className="text-foreground">
                        {new Date(imageData.storedAt).toLocaleString()}
                      </span>
                    </div>
                  </>
                )}
                {imageData.ownerIds && imageData.ownerIds.length > 0 && (
                  <>
                    <Separator className="bg-border/50" />
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Owners</span>
                      <span className="text-foreground">{imageData.ownerIds.length}</span>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>
          </div>

          {/* Right Side - Image */}
          <div className="space-y-4">
            {/* Title and Actions */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div className="flex items-center gap-3">
                <h1 className="font-display text-2xl md:text-3xl font-bold text-gradient">
                  Image {imageData.id.substring(0, 8)}
                </h1>
                {isOwner && (
                  <Badge 
                    variant="secondary" 
                    className="gap-1.5 bg-primary/20 text-primary border-primary/30"
                  >
                    <OwnerIcon />
                    You own this
                  </Badge>
                )}
              </div>
              <div className="flex items-center gap-2 flex-wrap">
                <Button variant="outline" size="sm" className="gap-2 border-border/50 hover:bg-primary/10 hover:text-primary hover:border-primary/50">
                  <HeartIcon />
                  Like
                </Button>
                <Button 
                  variant="outline" 
                  size="sm" 
                  className="gap-2 border-border/50 hover:bg-primary/10 hover:text-primary hover:border-primary/50"
                  onClick={handleDownload}
                  disabled={imageLoading}
                >
                  <DownloadIcon />
                  {imageLoading ? 'Downloading...' : 'Download'}
                </Button>
                <Button variant="outline" size="sm" className="gap-2 border-border/50 hover:bg-primary/10 hover:text-primary hover:border-primary/50">
                  <ShareIcon />
                </Button>
                <Button variant="outline" size="sm" className="gap-2 border-border/50 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/50">
                  <FlagIcon />
                </Button>
                {isOwner && (
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="gap-2 border-destructive/50 hover:bg-destructive/10 hover:text-destructive hover:border-destructive"
                    onClick={() => setDeleteDialogOpen(true)}
                    disabled={deleting}
                  >
                    <TrashIcon />
                    Delete
                  </Button>
                )}
              </div>
            </div>

            {/* Image Container */}
            <Card className="glass border-border/50 overflow-hidden">
              <div className="relative group">
                {imageLoading && (
                  <div className="absolute inset-0 flex items-center justify-center bg-background/80 z-10">
                    <div className="text-foreground">Loading original quality...</div>
                  </div>
                )}
                <img
                  src={getImageUrl()}
                  alt={`Image ${imageData.id}`}
                  className={cn(
                    "w-full h-auto object-contain max-h-[80vh]",
                    !useOriginalQuality && "cursor-pointer",
                    useOriginalQuality && "cursor-default"
                  )}
                  onClick={handleImageClick}
                  onLoad={handleImageLoad}
                  onError={handleImageError}
                />
                {!useOriginalQuality && (
                  <div className="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity">
                    <div className="px-3 py-2 rounded-md glass border border-border/50 bg-background/90 text-sm text-foreground">
                      Click to load original quality
                    </div>
                  </div>
                )}
                {useOriginalQuality && (
                  <a 
                    href={getImageUrl()} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity"
                  >
                    <Button size="sm" variant="secondary" className="gap-2 glass">
                      <ExternalLinkIcon />
                      Full Size
                    </Button>
                  </a>
                )}
              </div>
            </Card>
          </div>
        </div>
      </main>

      {error && (
        <ErrorPopup
          message={error}
          onClose={() => setError(null)}
        />
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Image</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete this image? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              disabled={deleting}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {deleting ? 'Deleting...' : 'Delete'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
