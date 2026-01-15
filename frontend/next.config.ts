import type { NextConfig } from "next";
import path from "path";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  output: 'standalone',
  // Next.js should automatically read path aliases from tsconfig.json
  // But we'll also configure webpack explicitly to ensure it works
  webpack: (config) => {
    const srcPath = path.resolve(__dirname, 'src');
    config.resolve.alias = {
      ...(config.resolve?.alias || {}),
      '@': srcPath,
    };
    return config;
  },
};

export default nextConfig;
