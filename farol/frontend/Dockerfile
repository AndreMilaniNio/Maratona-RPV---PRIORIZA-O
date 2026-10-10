# syntax=docker/dockerfile:1
# --- Etapa 1: build da SPA ---
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
# URL da API embutida no bundle no momento do build
ARG VITE_API_URL=http://localhost:5080
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

# --- Etapa 2: servir com nginx ---
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://127.0.0.1/ >/dev/null || exit 1
CMD ["nginx", "-g", "daemon off;"]
