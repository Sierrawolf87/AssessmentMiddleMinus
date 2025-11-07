FROM node:20.19-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi

FROM node:20.19-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

FROM node:20.19-alpine AS pruner
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY package*.json ./
RUN npm prune --omit=dev

FROM node:20.19-alpine AS runner
ENV NODE_ENV=production
WORKDIR /app

RUN apk add --no-cache tini
ENTRYPOINT ["/sbin/tini","--"]

COPY --from=pruner /app/node_modules ./node_modules
COPY --from=builder /app/dist ./dist
COPY package*.json ./

USER node

ENV PORT=3000
EXPOSE 3000

CMD ["node","dist/main.js"]
