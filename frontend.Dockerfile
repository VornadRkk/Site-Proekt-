FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
ARG VITE_API_URL=
ARG VITE_USE_MOCKS=false
ARG VITE_ENABLE_CLIENT_AUTH=true
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_USE_MOCKS=$VITE_USE_MOCKS
ENV VITE_ENABLE_CLIENT_AUTH=$VITE_ENABLE_CLIENT_AUTH
RUN npm run build

FROM nginx:1.28-alpine
COPY deploy/docker-nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
