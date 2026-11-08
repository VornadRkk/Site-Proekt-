FROM node:24-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ENV VITE_USE_MOCKS=false
ENV VITE_API_URL=""
RUN npm run build
FROM nginx:alpine
COPY deploy/nginx.local.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
