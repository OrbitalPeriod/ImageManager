# Stage 1: Dependencies
FROM node:20-alpine AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./
RUN npm ci

# Stage 2: Builder
FROM node:20-alpine AS builder
WORKDIR /app

# Copy dependencies from deps stage
COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Verify files are copied correctly (debug step)
RUN echo "=== Checking files ===" && \
    ls -la src/lib/api/ && \
    test -f src/lib/api/client-client.ts && echo "✓ client-client.ts exists" || echo "✗ client-client.ts missing" && \
    test -f src/lib/utils/cn.ts && echo "✓ cn.ts exists" || echo "✗ cn.ts missing" && \
    echo "=== Files check complete ==="

# Set build-time environment variable
# Note: NEXT_PUBLIC_ variables are embedded at build time in Next.js
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

# Build the application
RUN npm run build

# Stage 3: Runner
FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production

# Create a non-root user
RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy necessary files from builder
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

# Set ownership
RUN chown -R nextjs:nodejs /app

USER nextjs

EXPOSE 3000

ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Allow runtime override of API URL (for server-side usage)
# Note: Client-side code uses the value from build time
# This will use the value from build time, or can be overridden at runtime
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}

CMD ["node", "server.js"]
