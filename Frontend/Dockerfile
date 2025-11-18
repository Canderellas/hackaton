FROM node:22-alpine AS builder

WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine

# Копируем собранное приложение
COPY --from=builder /app/dist /usr/share/nginx/html

# Копируем nginx config
COPY nginx.conf /etc/nginx/nginx.conf

# Создаем папку для SSL сертификатов
RUN mkdir -p /etc/ssl/certs

EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]