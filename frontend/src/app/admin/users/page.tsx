/**
 * Admin Users Page
 * 
 * Admin-only page for managing users: viewing, approving, promoting, and demoting users.
 */

'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Copy, Check, Shield, ShieldOff, UserCheck, UserX } from 'lucide-react';
import { getClientApiClient } from '@/lib/api/client-client';
import { handleApiResponseError, handleError, getErrorMessage } from '@/lib/errors/handlers';
import { useAuth } from '@/lib/auth/context';
import { TopNavbar } from '@/components/layout/TopNavbar';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { ErrorPopup } from '@/components/ui/ErrorPopup';
import type { components } from '@/lib/api/client';

type UserListResponse = components['schemas']['UserListResponse'];

interface User {
  id: string;
  username: string;
  isAdmin: boolean;
  isApproved: boolean;
}

export default function AdminUsersPage() {
  const router = useRouter();
  const { user: currentUser, isLoading: authLoading } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [accessDenied, setAccessDenied] = useState(false);

  // Check if current user is admin
  const isAdmin = currentUser?.roles?.includes('Administrator') ?? false;

  const shortenId = (id: string) => {
    return `${id.substring(0, 8)}...`;
  };

  const copyToClipboard = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      await navigator.clipboard.writeText(id);
      setCopiedId(id);
      setTimeout(() => setCopiedId(null), 2000);
    } catch (err) {
      console.error('Failed to copy to clipboard:', err);
    }
  };

  // Map API response to UI User type
  const mapUserListResponseToUser = (apiUser: UserListResponse): User => {
    return {
      id: apiUser.id || '',
      username: apiUser.userName || '',
      isAdmin: apiUser.roles?.includes('Administrator') ?? false,
      isApproved: apiUser.isApproved ?? false,
    };
  };

  // Fetch users from API
  const fetchUsers = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const client = getClientApiClient();
      const response = await client.GET('/api/admin/users');

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      const userList = response.data as UserListResponse[] | undefined;
      if (userList) {
        const mappedUsers = userList.map(mapUserListResponseToUser);
        // Filter out the current user from the list
        const filteredUsers = currentUser?.id 
          ? mappedUsers.filter(user => user.id !== currentUser.id)
          : mappedUsers;
        setUsers(filteredUsers);
      } else {
        setUsers([]);
      }
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    } finally {
      setLoading(false);
    }
  }, [currentUser?.id]);

  // Initial fetch on mount - wait for auth to load first
  useEffect(() => {
    if (!authLoading) {
      // Check admin access immediately when auth loads
      if (!isAdmin) {
        setAccessDenied(true);
        setLoading(false);
        return;
      }
      // If admin, fetch users
      fetchUsers();
    }
  }, [fetchUsers, authLoading, isAdmin]);

  // Handle user row click
  const handleUserClick = (user: User) => {
    setSelectedUser(user);
    setIsDetailOpen(true);
  };

  // Approve user
  const approveUser = async () => {
    if (!selectedUser) return;

    try {
      const client = getClientApiClient();
      const response = await client.POST('/api/admin/users/{userId}/approve', {
        params: {
          path: {
            userId: selectedUser.id,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // Refresh user list
      await fetchUsers();
      
      // Update selected user
      setSelectedUser({ ...selectedUser, isApproved: true });
      
      // Close dialog after a short delay to show success
      setTimeout(() => {
        setIsDetailOpen(false);
        setSelectedUser(null);
      }, 500);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    }
  };

  // Promote user to admin
  const promoteUser = async () => {
    if (!selectedUser) return;

    try {
      const client = getClientApiClient();
      const response = await client.POST('/api/admin/users/{userId}/promote', {
        params: {
          path: {
            userId: selectedUser.id,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // Refresh user list
      await fetchUsers();
      
      // Update selected user
      setSelectedUser({ ...selectedUser, isAdmin: true });
      
      // Close dialog after a short delay to show success
      setTimeout(() => {
        setIsDetailOpen(false);
        setSelectedUser(null);
      }, 500);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    }
  };

  // Demote user from admin
  const demoteUser = async () => {
    if (!selectedUser) return;

    try {
      const client = getClientApiClient();
      const response = await client.POST('/api/admin/users/{userId}/demote', {
        params: {
          path: {
            userId: selectedUser.id,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // Refresh user list
      await fetchUsers();
      
      // Update selected user
      setSelectedUser({ ...selectedUser, isAdmin: false });
      
      // Close dialog after a short delay to show success
      setTimeout(() => {
        setIsDetailOpen(false);
        setSelectedUser(null);
      }, 500);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    }
  };

  // Unapprove user (toggle approval status)
  const unapproveUser = async () => {
    if (!selectedUser) return;

    try {
      const client = getClientApiClient();
      const response = await client.POST('/api/admin/users/{userId}/toggle-approval', {
        params: {
          path: {
            userId: selectedUser.id,
          },
        },
      });

      if (response.error) {
        const appError = handleApiResponseError(response);
        setError(getErrorMessage(appError));
        return;
      }

      // Refresh user list
      await fetchUsers();
      
      // Update selected user
      setSelectedUser({ ...selectedUser, isApproved: false });
      
      // Close dialog after a short delay to show success
      setTimeout(() => {
        setIsDetailOpen(false);
        setSelectedUser(null);
      }, 500);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    }
  };

  // Handle access denied - redirect to home
  useEffect(() => {
    if (accessDenied) {
      setTimeout(() => {
        router.push('/');
      }, 2000);
    }
  }, [accessDenied, router]);

  // Loading state (wait for auth to load first)
  if (authLoading || loading) {
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

  // Access denied state
  if (accessDenied) {
    return (
      <div className="min-h-screen bg-background">
        <TopNavbar />
        <main className="container mx-auto px-4 py-8">
          <div className="flex items-center justify-center py-20">
            <div className="text-foreground text-lg text-center">
              <h2 className="text-2xl font-bold mb-4">Access Denied</h2>
              <p className="text-muted-foreground">You do not have permission to access this page.</p>
              <p className="text-muted-foreground mt-2">Redirecting to home...</p>
            </div>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <TopNavbar />
      <main className="container mx-auto px-4 py-8">
        <div className="p-6 rounded-xl border border-border">
          <div className="flex items-center justify-between mb-6">
            <h1 className="text-2xl font-bold text-foreground">User Management</h1>
            <Badge variant="outline" className="text-muted-foreground">
              {users.length} users
            </Badge>
          </div>

          <div className="rounded-lg border border-border overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/50">
                  <TableHead className="text-foreground">ID</TableHead>
                  <TableHead className="text-foreground">Username</TableHead>
                  <TableHead className="text-foreground">Admin</TableHead>
                  <TableHead className="text-foreground">Approved</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {users.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={4} className="text-center text-muted-foreground py-8">
                      No users found
                    </TableCell>
                  </TableRow>
                ) : (
                  users.map((user) => (
                    <TableRow
                      key={user.id}
                      className="cursor-pointer hover:bg-muted/30 transition-colors"
                      onClick={() => handleUserClick(user)}
                    >
                      <TableCell className="font-mono text-sm">
                        <div className="flex items-center gap-2">
                          <span className="text-muted-foreground">{shortenId(user.id)}</span>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6"
                            onClick={(e) => copyToClipboard(user.id, e)}
                          >
                            {copiedId === user.id ? (
                              <Check className="h-3 w-3 text-green-500" />
                            ) : (
                              <Copy className="h-3 w-3" />
                            )}
                          </Button>
                        </div>
                      </TableCell>
                      <TableCell className="font-medium text-foreground">
                        {user.username}
                      </TableCell>
                      <TableCell>
                        {user.isAdmin ? (
                          <Badge className="bg-primary/20 text-primary border-primary/30">
                            <Shield className="h-3 w-3 mr-1 inline" />
                            Admin
                          </Badge>
                        ) : (
                          <Badge variant="outline" className="text-muted-foreground">
                            User
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        {user.isApproved ? (
                          <Badge className="bg-green-500/20 text-green-400 border-green-500/30">
                            Approved
                          </Badge>
                        ) : (
                          <Badge className="bg-yellow-500/20 text-yellow-400 border-yellow-500/30">
                            Pending
                          </Badge>
                        )}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </div>

        {/* User Detail Dialog */}
        <Dialog open={isDetailOpen} onOpenChange={setIsDetailOpen}>
          <DialogContent className="border-border">
            <DialogHeader>
              <DialogTitle className="text-foreground">User Details</DialogTitle>
              <DialogDescription className="text-muted-foreground">
                Manage user permissions and approval status
              </DialogDescription>
            </DialogHeader>
            
            {selectedUser && (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">ID:</span>
                    <p className="font-mono text-foreground break-all">{selectedUser.id}</p>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Username:</span>
                    <p className="font-medium text-foreground">{selectedUser.username}</p>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Role:</span>
                    <p className="text-foreground">{selectedUser.isAdmin ? "Admin" : "User"}</p>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Status:</span>
                    <p className="text-foreground">{selectedUser.isApproved ? "Approved" : "Pending Approval"}</p>
                  </div>
                </div>
              </div>
            )}

            <DialogFooter className="flex gap-2 sm:gap-2">
              {selectedUser && !selectedUser.isApproved && (
                <Button
                  onClick={approveUser}
                  className="bg-green-600 hover:bg-green-700"
                >
                  <UserCheck className="h-4 w-4 mr-2" />
                  Approve User
                </Button>
              )}
              
              {selectedUser && selectedUser.isApproved && (
                <Button
                  onClick={unapproveUser}
                  variant="destructive"
                >
                  <UserX className="h-4 w-4 mr-2" />
                  Unapprove User
                </Button>
              )}
              
              {selectedUser && (
                <Button
                  variant={selectedUser.isAdmin ? "destructive" : "default"}
                  onClick={selectedUser.isAdmin ? demoteUser : promoteUser}
                >
                  {selectedUser.isAdmin ? (
                    <>
                      <ShieldOff className="h-4 w-4 mr-2" />
                      Demote from Admin
                    </>
                  ) : (
                    <>
                      <Shield className="h-4 w-4 mr-2" />
                      Promote to Admin
                    </>
                  )}
                </Button>
              )}
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </main>

      {error && (
        <ErrorPopup
          message={error}
          onClose={() => setError(null)}
        />
      )}
    </div>
  );
}
