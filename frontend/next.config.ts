import type { NextConfig } from "next";
import path from "path";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  output: 'standalone',
  webpack: (config) => {
    // Explicitly configure path alias to match tsconfig.json
    const srcPath = path.resolve(__dirname, 'src');
    
    // Ensure resolve object exists
    config.resolve = config.resolve || {};
    config.resolve.alias = {
      ...(config.resolve.alias || {}),
      '@': srcPath,
    };
    
    // Ensure extensions include .ts and .tsx
    config.resolve.extensions = [
      ...(config.resolve.extensions || []),
      '.ts',
      '.tsx',
      '.js',
      '.jsx',
    ];
    
    return config;
  },
};

export default nextConfig;
