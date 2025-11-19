FROM node:22-alpine

WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Keep container running
CMD ["tail", "-f", "/dev/null"]