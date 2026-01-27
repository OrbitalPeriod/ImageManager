/**
 * Folders Management Page
 * Allows users to view, create, and delete folders
 */

'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleApiResponseError, handleError, getErrorMessage } from '@/lib/errors/handlers';
import { useAuth } from '@/lib/auth/context';
import { TopNavbar } from '@/components/layout/TopNavbar';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ErrorPopup } from '@/components/ui/ErrorPopup';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
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
import type { components } from '@/lib/api/client';

type FolderDto = components['schemas']['FolderDto'];

const FolderIcon = () => (
  <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
  </svg>
);

const PlusIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
  </svg>
);

const TrashIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
  </svg>
);

export default function FoldersPage() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [folders, setFolders] = useState<FolderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [newFolderName, setNewFolderName] = useState('');
  const [creating, setCreating] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [folderToDelete, setFolderToDelete] = useState<FolderDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Redirect if not authenticated
  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isAuthenticated, authLoading, router]);

  // Fetch folders
  const fetchFolders = useCallback(async () => {
    if (!isAuthenticated) return;

    setLoading(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/folders');

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
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
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      setFolders([]);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchFolders();
    }
  }, [isAuthenticated, fetchFolders]);

  // Create folder
  const handleCreateFolder = async () => {
    if (!newFolderName.trim()) {
      setError('Folder name cannot be empty');
      return;
    }

    setCreating(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.POST('/api/folders', {
        body: {
          name: newFolderName.trim(),
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setCreating(false);
        return;
      }

      // Success - refresh folders and close dialog
      setNewFolderName('');
      setCreateDialogOpen(false);
      await fetchFolders();
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    } finally {
      setCreating(false);
    }
  };

  // Delete folder
  const handleDeleteClick = (folder: FolderDto) => {
    setFolderToDelete(folder);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!folderToDelete?.id) return;

    setDeleting(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.DELETE('/api/folders/{folderId}', {
        params: {
          path: {
            folderId: folderToDelete.id,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setDeleting(false);
        setDeleteDialogOpen(false);
        setFolderToDelete(null);
        return;
      }

      // Success - refresh folders and close dialog
      setDeleteDialogOpen(false);
      setFolderToDelete(null);
      await fetchFolders();
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    } finally {
      setDeleting(false);
    }
  };

  if (authLoading || !isAuthenticated) {
    return (
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main className="container mx-auto px-4 py-6">
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading...</div>
          </div>
        </main>
      </div>
    );
  }

  const isLikedFolder = (folder: FolderDto) => folder.name === 'Liked';

  return (
    <div className="min-h-screen bg-background">
      <TopNavbar />
      <main className="container mx-auto px-4 py-6">
        <div className="flex items-center justify-between mb-6">
          <h1 className="font-display text-3xl font-bold text-gradient">My Folders</h1>
          <Button
            onClick={() => setCreateDialogOpen(true)}
            className="gap-2"
          >
            <PlusIcon />
            Create Folder
          </Button>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading folders...</div>
          </div>
        ) : folders.length === 0 ? (
          <Card className="glass border-border/50">
            <CardContent className="flex flex-col items-center justify-center py-12">
              <FolderIcon />
              <p className="mt-4 text-muted-foreground">No folders yet. Create your first folder to get started!</p>
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {folders.map((folder) => (
              <Card key={folder.id} className="glass border-border/50 hover:border-primary/50 transition-colors">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2">
                      <FolderIcon />
                      {folder.name || 'Unnamed Folder'}
                    </CardTitle>
                    {!isLikedFolder(folder) && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeleteClick(folder)}
                        className="text-destructive hover:text-destructive hover:bg-destructive/10"
                        title="Delete folder"
                      >
                        <TrashIcon />
                      </Button>
                    )}
                  </div>
                </CardHeader>
                <CardContent>
                  <Button
                    variant="outline"
                    className="w-full"
                    onClick={() => router.push(`/?folders=${folder.id}`)}
                  >
                    View Images
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>
        )}

        {/* Create Folder Dialog */}
        <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create New Folder</DialogTitle>
              <DialogDescription>
                Enter a name for your new folder. You can organize your images by adding them to folders.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="folder-name">Folder Name</Label>
                <Input
                  id="folder-name"
                  value={newFolderName}
                  onChange={(e) => setNewFolderName(e.target.value)}
                  placeholder="My Collection"
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && !creating) {
                      handleCreateFolder();
                    }
                  }}
                />
              </div>
            </div>
            <DialogFooter>
              <Button
                variant="outline"
                onClick={() => {
                  setCreateDialogOpen(false);
                  setNewFolderName('');
                }}
                disabled={creating}
              >
                Cancel
              </Button>
              <Button onClick={handleCreateFolder} disabled={creating || !newFolderName.trim()}>
                {creating ? 'Creating...' : 'Create'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* Delete Confirmation Dialog */}
        <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Delete Folder</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to delete "{folderToDelete?.name}"? This will not delete the images in the folder, only remove the folder organization.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDeleteConfirm}
                disabled={deleting}
                className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              >
                {deleting ? 'Deleting...' : 'Delete'}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        {error && (
          <ErrorPopup
            message={error}
            onClose={() => setError(null)}
          />
        )}
      </main>
    </div>
  );
}
