FROM node:26-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Sem root: esta imagem roda como usuário comum e, por isso, escuta na 8080.
FROM nginxinc/nginx-unprivileged:1.31-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/biblia-teologica-web/browser /usr/share/nginx/html
EXPOSE 8080
