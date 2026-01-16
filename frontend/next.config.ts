import type { NextConfig } from "next";
import path from "path";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  output: 'standalone',
  turbopack: {},
  webpack: (config, { buildId, dev, isServer, defaultLoaders, webpack }) => {
    // Explicitly configure path alias to match tsconfig.json
    const srcPath = path.resolve(__dirname, 'src');
    
    // Log for debugging (will show in build output)
    console.log('[Webpack Config] Setting alias @ to:', srcPath);
    console.log('[Webpack Config] Files exist check:', require('fs').existsSync(path.join(srcPath, 'lib/api/client-client.ts')));
    
    // Ensure resolve object exists
    if (!config.resolve) {
      config.resolve = {};
    }
    
    // Set up aliases - this should handle @/ imports
    const existingAliases = config.resolve.alias || {};
    config.resolve.alias = {
      ...existingAliases,
      '@': srcPath,
    };
    
    // Log the final alias configuration
    console.log('[Webpack Config] Final aliases:', config.resolve.alias);
    
    return config;
  },
};

export default nextConfig;
