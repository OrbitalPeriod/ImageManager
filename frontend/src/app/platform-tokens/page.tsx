'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Trash2, Eye, Key, RefreshCw } from 'lucide-react';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleApiResponseError, handleError, getErrorMessage } from '@/lib/errors/handlers';
import { useAuth } from '@/lib/auth/context';
import type { components } from '@/lib/api/client';
import { TopNavbar } from '@/components/layout/TopNavbar';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
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
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { ErrorPopup } from '@/components/ui/ErrorPopup';

type PlatformTokenDto = components['schemas']['PlatformTokenDto'];
type PlatformTokenSyncLog = components['schemas']['PlatformTokenSyncLog'];
type AddTokenRequest = components['schemas']['AddTokenRequest'];

export default function PlatformTokensPage() {
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [tokens, setTokens] = useState<PlatformTokenDto[]>([]);
  const [selectedToken, setSelectedToken] = useState<PlatformTokenDto | null>(null);
  const [logs, setLogs] = useState<PlatformTokenSyncLog[]>([]);
  const [showLogsDialog, setShowLogsDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [loading, setLoading] = useState(true);
  const [loadingLogs, setLoadingLogs] = useState(false);
  const [syncing, setSyncing] = useState<string | null>(null); // Track which token is syncing
  const [error, setError] = useState<string | null>(null);

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      const currentPath = window.location.pathname;
      router.push(`/login?returnUrl=${encodeURIComponent(currentPath)}`);
    }
  }, [isAuthenticated, authLoading, router]);
  
  // New token form state
  const [newToken, setNewToken] = useState<AddTokenRequest>({
    token: null,
    platformUserId: null,
    expires: null,
    platform: 0, // Platform 0 = Pixiv
    checkPrivate: true,
  });

  // Fetch tokens on mount (only if authenticated)
  const fetchTokens = useCallback(async () => {
    if (!isAuthenticated) return;

    setLoading(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/platform-tokens');

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setTokens([]);
        return;
      }

      const data = response.data as PlatformTokenDto[] | undefined;
      setTokens(data || []);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      setTokens([]);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated && !authLoading) {
      fetchTokens();
    }
  }, [fetchTokens, isAuthenticated, authLoading]);

  // Fetch logs for a token
  const fetchLogs = useCallback(async (tokenId: string) => {
    if (!tokenId) return;

    setLoadingLogs(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/platform-tokens/{id}/logs', {
        params: {
          path: { id: tokenId },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        setLogs([]);
        return;
      }

      const data = response.data as PlatformTokenSyncLog[] | undefined;
      // Sort logs by createdAt (newest first)
      const sortedLogs = (data || []).sort((a, b) => {
        const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
        const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
        return dateB - dateA; // Descending order (newest first)
      });
      setLogs(sortedLogs);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
      setLogs([]);
    } finally {
      setLoadingLogs(false);
    }
  }, []);

  const handleViewLogs = async (token: PlatformTokenDto) => {
    setSelectedToken(token);
    setShowLogsDialog(true);
    if (token.id) {
      await fetchLogs(token.id);
    }
  };

  const handleDeleteClick = () => {
    setShowDeleteDialog(true);
  };

  const handleDeleteConfirm = async () => {
    if (!selectedToken?.id) return;

    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.DELETE('/api/platform-tokens/{id}', {
        params: {
          path: { id: selectedToken.id },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // Refresh tokens list
      await fetchTokens();
      setShowDeleteDialog(false);
      setShowLogsDialog(false);
      setSelectedToken(null);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    }
  };

  const handleSync = async (tokenId: string) => {
    if (!tokenId) return;

    setSyncing(tokenId);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.POST('/api/platform-tokens/{id}/run', {
        params: {
          path: { id: tokenId },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // If logs dialog is open for this token, refresh the logs
      if (selectedToken?.id === tokenId) {
        await fetchLogs(tokenId);
      }
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    } finally {
      setSyncing(null);
    }
  };

  const handleAddToken = async () => {
    setError(null);

    // Convert datetime-local to ISO string
    let expiresISO: string | null = null;
    if (newToken.expires) {
      // If expires is in datetime-local format, convert it
      const date = new Date(newToken.expires);
      if (!isNaN(date.getTime())) {
        expiresISO = date.toISOString();
      }
    }

    const requestBody: AddTokenRequest = {
      token: newToken.token || null,
      platformUserId: newToken.platformUserId || null,
      expires: expiresISO,
      platform: newToken.platform ?? 0,
      checkPrivate: newToken.checkPrivate ?? true,
    };

    try {
      const client = getClientApiClient();
      const response = await client.PUT('/api/platform-tokens/add', {
        body: requestBody,
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // Refresh tokens list
      await fetchTokens();
      setShowAddDialog(false);
      setNewToken({
        token: null,
        platformUserId: null,
        expires: null,
        platform: 0,
        checkPrivate: true,
      });
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    }
  };

  const formatDate = (dateString: string | null | undefined) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString();
  };

  // Convert ISO date to datetime-local format
  const isoToDatetimeLocal = (isoString: string | null | undefined): string => {
    if (!isoString) return '';
    const date = new Date(isoString);
    if (isNaN(date.getTime())) return '';
    // Format as YYYY-MM-DDTHH:mm
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const getPlatformName = (platform: number | undefined): string => {
    // Platform 0 = Pixiv
    if (platform === 0) return 'Pixiv';
    return `Platform ${platform}`;
  };

  // Show loading state while checking authentication
  if (authLoading) {
    return (
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main className="container mx-auto px-4 py-8">
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading...</div>
          </div>
        </main>
      </div>
    );
  }

  // Don't render if not authenticated (will redirect)
  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="min-h-screen bg-background">
      <TopNavbar />
      
      <main className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-lg bg-primary/20 flex items-center justify-center glow-primary-sm">
              <Key className="h-5 w-5 text-primary" />
            </div>
            <div>
              <h1 className="font-display text-2xl font-bold text-gradient">Platform Tokens</h1>
              <p className="text-sm text-muted-foreground">Manage your platform authentication tokens</p>
            </div>
          </div>
          
          <Dialog open={showAddDialog} onOpenChange={setShowAddDialog}>
            <DialogTrigger asChild>
              <Button className="gap-2 glow-primary-sm">
                <Plus className="h-4 w-4" />
                Add Token
              </Button>
            </DialogTrigger>
            <DialogContent className="glass border-border">
              <DialogHeader>
                <DialogTitle className="font-display text-gradient">Add New Platform Token</DialogTitle>
                <DialogDescription>
                  Add a new authentication token for a platform.
                </DialogDescription>
              </DialogHeader>
              
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label htmlFor="token">Token</Label>
                  <Input
                    id="token"
                    type="password"
                    placeholder="Enter token..."
                    value={newToken.token || ''}
                    onChange={(e) => setNewToken({ ...newToken, token: e.target.value || null })}
                    className="bg-secondary/50 border-border"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="platformUserId">Platform User ID</Label>
                  <Input
                    id="platformUserId"
                    placeholder="user_12345"
                    value={newToken.platformUserId || ''}
                    onChange={(e) => setNewToken({ ...newToken, platformUserId: e.target.value || null })}
                    className="bg-secondary/50 border-border"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="expires">Expires</Label>
                  <Input
                    id="expires"
                    type="datetime-local"
                    value={newToken.expires ? isoToDatetimeLocal(newToken.expires) : ''}
                    onChange={(e) => {
                      const value = e.target.value;
                      // Convert datetime-local to ISO string
                      if (value) {
                        const date = new Date(value);
                        setNewToken({ ...newToken, expires: date.toISOString() });
                      } else {
                        setNewToken({ ...newToken, expires: null });
                      }
                    }}
                    className="bg-secondary/50 border-border"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="platform">Platform</Label>
                  <Input
                    id="platform"
                    value="Pixiv"
                    disabled
                    className="bg-secondary/50 border-border opacity-50"
                  />
                </div>
                
                <div className="flex items-center justify-between">
                  <Label htmlFor="checkPrivate">Check Private</Label>
                  <Switch
                    id="checkPrivate"
                    checked={newToken.checkPrivate ?? true}
                    onCheckedChange={(checked) => setNewToken({ ...newToken, checkPrivate: checked })}
                  />
                </div>
              </div>
              
              <DialogFooter>
                <Button variant="outline" onClick={() => setShowAddDialog(false)}>
                  Cancel
                </Button>
                <Button onClick={handleAddToken} className="glow-primary-sm">
                  Add Token
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </div>

        {/* Tokens Table */}
        {loading ? (
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg">Loading tokens...</div>
          </div>
        ) : (
          <div className="glass rounded-xl border border-border/50 overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow className="border-border/50 hover:bg-transparent">
                  <TableHead className="text-muted-foreground">ID</TableHead>
                  <TableHead className="text-muted-foreground">Platform</TableHead>
                  <TableHead className="text-muted-foreground">Platform User ID</TableHead>
                  <TableHead className="text-muted-foreground">Expires</TableHead>
                  <TableHead className="text-muted-foreground">Status</TableHead>
                  <TableHead className="text-muted-foreground">Check Private</TableHead>
                  <TableHead className="text-muted-foreground text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {tokens.map((token) => (
                  <TableRow key={token.id} className="border-border/50 hover:bg-primary/5">
                    <TableCell className="font-mono text-sm">{token.id}</TableCell>
                    <TableCell>
                      <Badge variant="outline" className="border-primary/50 text-primary">
                        {getPlatformName(token.platform)}
                      </Badge>
                    </TableCell>
                    <TableCell className="font-mono text-sm">{token.platformUserId || 'N/A'}</TableCell>
                    <TableCell className="text-sm">{formatDate(token.expires)}</TableCell>
                    <TableCell>
                      <Badge 
                        variant={token.isExpired ? "destructive" : "default"} 
                        className={!token.isExpired ? "bg-green-500/20 text-green-400 border-green-500/50" : ""}
                      >
                        {token.isExpired ? "Expired" : "Active"}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge 
                        variant="outline" 
                        className={token.checkPrivate ? "border-primary/50 text-primary" : "border-muted-foreground/50 text-muted-foreground"}
                      >
                        {token.checkPrivate ? "Yes" : "No"}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="gap-2 hover:bg-primary/10 hover:text-primary"
                          onClick={() => handleSync(token.id!)}
                          disabled={!token.id || syncing === token.id}
                        >
                          <RefreshCw className={`h-4 w-4 ${syncing === token.id ? 'animate-spin' : ''}`} />
                          Sync Now
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="gap-2 hover:bg-primary/10 hover:text-primary"
                          onClick={() => handleViewLogs(token)}
                        >
                          <Eye className="h-4 w-4" />
                          View Logs
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
                {tokens.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-8 text-muted-foreground">
                      No platform tokens found. Add one to get started.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>
        )}
      </main>

      {/* Logs Dialog */}
      <Dialog open={showLogsDialog} onOpenChange={setShowLogsDialog}>
        <DialogContent className="glass border-border max-w-2xl">
          <DialogHeader>
            <DialogTitle className="font-display text-gradient">
              Token Logs - {selectedToken?.platformUserId || 'N/A'}
            </DialogTitle>
            <DialogDescription>
              View activity logs for this platform token.
            </DialogDescription>
          </DialogHeader>
          
          {loadingLogs ? (
            <div className="flex items-center justify-center py-20">
              <div className="text-foreground text-lg">Loading logs...</div>
            </div>
          ) : (
            <ScrollArea className="h-[300px] rounded-lg border border-border/50 bg-secondary/30 p-4">
              <div className="space-y-3">
                {logs.length === 0 ? (
                  <div className="text-center text-muted-foreground py-8">
                    No logs available for this token.
                  </div>
                ) : (
                  logs.map((log) => (
                    <div
                      key={log.id}
                      className={`p-3 rounded-lg border ${
                        log.success
                          ? "border-green-500/30 bg-green-500/10"
                          : "border-destructive/30 bg-destructive/10"
                      }`}
                    >
                      <div className="flex items-center justify-between mb-1">
                        <Badge
                          variant={log.success ? "default" : "destructive"}
                          className={log.success ? "bg-green-500/20 text-green-400 border-green-500/50" : ""}
                        >
                          {log.success ? "Success" : "Failed"}
                        </Badge>
                        <span className="text-xs text-muted-foreground">
                          {formatDate(log.createdAt)}
                        </span>
                      </div>
                      <p className="text-sm">{log.message || 'No message'}</p>
                    </div>
                  ))
                )}
              </div>
            </ScrollArea>
          )}
          
          <DialogFooter className="flex-row justify-between sm:justify-between">
            <div className="flex gap-2">
              <Button
                variant="outline"
                className="gap-2"
                onClick={() => selectedToken?.id && handleSync(selectedToken.id)}
                disabled={!selectedToken?.id || syncing === selectedToken.id}
              >
                <RefreshCw className={`h-4 w-4 ${syncing === selectedToken?.id ? 'animate-spin' : ''}`} />
                Sync Now
              </Button>
              <Button
                variant="destructive"
                className="gap-2"
                onClick={handleDeleteClick}
              >
                <Trash2 className="h-4 w-4" />
                Delete Token
              </Button>
            </div>
            <Button variant="outline" onClick={() => setShowLogsDialog(false)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent className="glass border-border">
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Platform Token?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the token
              for <span className="font-mono text-primary">{selectedToken?.platformUserId || 'N/A'}</span>.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteConfirm}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Error Popup */}
      {error && (
        <ErrorPopup
          message={error}
          onClose={() => setError(null)}
        />
      )}
    </div>
  );
}
