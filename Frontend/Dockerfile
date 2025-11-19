FROM node:22-alpine AS builder

WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# ТОЛЬКО сборка, без nginx!
# Не копируем nginx.conf и не используем второй FROMs