/**
 * Top Navigation Bar Component
 * Based on the example Header design
 */

'use client';

import React from 'react';
import Link from 'next/link';
import { cn } from '@/lib/utils/cn';

const UploadIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
  </svg>
);

const TagsIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
  </svg>
);

const UserIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
  </svg>
);

const SettingsIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
  </svg>
);

const LogOutIcon = () => (
  <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
  </svg>
);

export function TopNavbar() {
  const [dropdownOpen, setDropdownOpen] = React.useState(false);

  return (
    <header className="sticky top-0 z-50 w-full glass border-b border-border/50">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        {/* Logo */}
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-lg bg-primary/20 flex items-center justify-center glow-primary-sm">
            <span className="text-primary font-display font-bold text-lg">A</span>
          </div>
          <Link href="/" className="font-display text-xl font-semibold text-gradient">
            ImageManager
          </Link>
        </div>

        {/* Navigation */}
        <nav className="hidden md:flex items-center gap-2">
          <Link
            href="/upload"
            className="inline-flex items-center gap-2 px-3 py-2 rounded-md text-sm font-medium hover:bg-primary/10 hover:text-primary transition-colors"
          >
            <UploadIcon />
            Upload
          </Link>
          <Link
            href="/tags"
            className="inline-flex items-center gap-2 px-3 py-2 rounded-md text-sm font-medium hover:bg-primary/10 hover:text-primary transition-colors"
          >
            <TagsIcon />
            Browse Tags
          </Link>
        </nav>

        {/* Profile Dropdown */}
        <div className="relative">
          <button
            onClick={() => setDropdownOpen(!dropdownOpen)}
            className="relative h-10 w-10 rounded-full ring-2 ring-primary/30 hover:ring-primary transition-all flex items-center justify-center bg-secondary"
          >
            <div className="h-9 w-9 rounded-full bg-secondary text-secondary-foreground flex items-center justify-center">
              <UserIcon />
            </div>
          </button>

          {dropdownOpen && (
            <>
              <div
                className="fixed inset-0 z-40"
                onClick={() => setDropdownOpen(false)}
              />
              <div className="absolute right-0 mt-2 w-56 z-50 glass rounded-md border border-border shadow-lg">
                <div className="flex items-center gap-2 p-2">
                  <div className="h-8 w-8 rounded-full bg-secondary text-secondary-foreground flex items-center justify-center">
                    <UserIcon />
                  </div>
                  <div className="flex flex-col space-y-0.5">
                    <p className="text-sm font-medium">User</p>
                    <p className="text-xs text-muted-foreground">user@example.com</p>
                  </div>
                </div>
                <div className="h-px bg-muted" />
                <Link
                  href="/user"
                  className="flex items-center gap-2 px-2 py-1.5 text-sm cursor-pointer hover:bg-primary/10 focus:bg-primary/10 rounded-sm"
                  onClick={() => setDropdownOpen(false)}
                >
                  <UserIcon />
                  View Profile
                </Link>
                <Link
                  href="/settings"
                  className="flex items-center gap-2 px-2 py-1.5 text-sm cursor-pointer hover:bg-primary/10 focus:bg-primary/10 rounded-sm"
                  onClick={() => setDropdownOpen(false)}
                >
                  <SettingsIcon />
                  Settings
                </Link>
                <div className="h-px bg-muted" />
                <button
                  className="flex items-center gap-2 px-2 py-1.5 text-sm cursor-pointer text-destructive hover:bg-destructive/10 focus:bg-destructive/10 rounded-sm w-full text-left"
                  onClick={() => setDropdownOpen(false)}
                >
                  <LogOutIcon />
                  Sign Out
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
}