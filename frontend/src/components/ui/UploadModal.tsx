/**
 * Upload Modal Component
 * 
 * A modal component for uploading images with publicity settings.
 */

'use client';

import React, { useState, useRef } from 'react';
import { createPortal } from 'react-dom';
import { useAuth } from '@/lib/auth/context';
import { Button } from '@/components/ui/button';
import { handleError, getErrorMessage } from '@/lib/errors/handlers';
import { ErrorPopup } from '@/components/ui/ErrorPopup';

interface UploadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: (imageId: string) => void;
}

type PublicityLevel = 0 | 1 | 2;

const PUBLICITY_OPTIONS: { value: PublicityLevel; label: string }[] = [
  { value: 0, label: 'Open' },
  { value: 1, label: 'Restricted' },
  { value: 2, label: 'Private' },
];

export function UploadModal({ isOpen, onClose, onSuccess }: UploadModalProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [publicity, setPublicity] = useState<PublicityLevel>(0);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { isAuthenticated } = useAuth();

  const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || '';

  // Reset state when modal closes
  React.useEffect(() => {
    if (!isOpen) {
      setSelectedFile(null);
      setPublicity(0);
      setError(null);
      setSuccess(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  }, [isOpen]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        setError('Please select an image file');
        return;
      }
      setSelectedFile(file);
      setError(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Please select a file to upload');
      return;
    }

    if (!isAuthenticated) {
      setError('You must be logged in to upload images');
      return;
    }

    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append('File', selectedFile);
      formData.append('Publicity', publicity.toString());

      const response = await fetch(`${apiBaseUrl}/api/images`, {
        method: 'PUT',
        body: formData,
        credentials: 'include', // Include auth cookies
      });

      if (!response.ok) {
        let errorMessage = 'Failed to upload image';
        
        // Try to parse error response
        try {
          const errorData = await response.json();
          if (errorData.detail) {
            errorMessage = errorData.detail;
          } else if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.title) {
            errorMessage = errorData.title;
          }
        } catch {
          // If JSON parsing fails, use status text
          errorMessage = response.statusText || `Server error (${response.status})`;
        }

        if (response.status === 401) {
          errorMessage = 'Unauthorized. Please log in again.';
        } else if (response.status === 422) {
          errorMessage = errorMessage || 'Invalid file format or data';
        }

        throw new Error(errorMessage);
      }

      // Get the image ID from response (UUID string)
      const imageId = await response.text();
      
      setSuccess(true);
      
      // Call success callback if provided
      if (onSuccess) {
        onSuccess(imageId);
      }

      // Close modal after a short delay to show success message
      setTimeout(() => {
        onClose();
      }, 1500);
    } catch (err) {
      const appError = handleError(err);
      setError(getErrorMessage(appError));
    } finally {
      setUploading(false);
    }
  };

  const handleClose = () => {
    if (!uploading) {
      onClose();
    }
  };

  const [mounted, setMounted] = useState(false);

  React.useEffect(() => {
    setMounted(true);
  }, []);

  if (!isOpen || !mounted) return null;

  const modalContent = (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black bg-opacity-50"
      onClick={handleClose}
    >
        <div
          className="relative bg-white dark:bg-gray-800 rounded-lg shadow-xl p-6 max-w-md w-full mx-4"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-start justify-between mb-4">
            <h3 className="text-lg font-semibold text-foreground">Upload Image</h3>
            <button
              onClick={handleClose}
              className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
              aria-label="Close"
              disabled={uploading}
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          {success ? (
            <div className="text-center py-4">
              <div className="text-green-600 dark:text-green-400 mb-2">
                <svg
                  className="w-12 h-12 mx-auto"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              </div>
              <p className="text-foreground font-medium">Image uploaded successfully!</p>
            </div>
          ) : (
            <div className="space-y-4">
              {/* File Input */}
              <div>
                <label className="block text-sm font-medium text-foreground mb-2">
                  Select Image
                </label>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*"
                  onChange={handleFileChange}
                  disabled={uploading}
                  className="block w-full text-sm text-foreground file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-primary file:text-primary-foreground hover:file:bg-primary/90 file:cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                />
                {selectedFile && (
                  <p className="mt-2 text-sm text-muted-foreground">
                    Selected: {selectedFile.name} ({(selectedFile.size / 1024 / 1024).toFixed(2)} MB)
                  </p>
                )}
              </div>

              {/* Publicity Selection */}
              <div>
                <label className="block text-sm font-medium text-foreground mb-2">
                  Publicity Level
                </label>
                <div className="space-y-2">
                  {PUBLICITY_OPTIONS.map((option) => (
                    <label
                      key={option.value}
                      className="flex items-center space-x-2 cursor-pointer"
                    >
                      <input
                        type="radio"
                        name="publicity"
                        value={option.value}
                        checked={publicity === option.value}
                        onChange={(e) => setPublicity(Number(e.target.value) as PublicityLevel)}
                        disabled={uploading}
                        className="w-4 h-4 text-primary focus:ring-primary border-gray-300"
                      />
                      <span className="text-sm text-foreground">{option.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              {/* Error Message */}
              {error && (
                <div className="p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-md">
                  <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                </div>
              )}

              {/* Action Buttons */}
              <div className="flex justify-end gap-2 pt-4">
                <Button
                  variant="outline"
                  onClick={handleClose}
                  disabled={uploading}
                >
                  Cancel
                </Button>
                <Button
                  onClick={handleUpload}
                  disabled={uploading || !selectedFile}
                >
                  {uploading ? 'Uploading...' : 'Upload'}
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
  );

  return createPortal(modalContent, document.body);
}
