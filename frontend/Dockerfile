# Build de React con Node 22 (Debian; evita alpine para bindings nativos)
FROM node:22 AS build
WORKDIR /app

# Copia SOLO package.json primero (NO copies package-lock.json)
COPY package.json ./

# Borra cualquier lockfile si se copió por error y evita usarlo en Linux
RUN rm -f package-lock.json

# Instala dependencias resolviendo para Linux
RUN npm install

# Copia el resto del código
COPY . .

# Construye el proyecto
RUN npm run build

# ---- Runtime: NGINX ----
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
