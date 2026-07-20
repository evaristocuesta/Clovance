FROM node:24 AS build

WORKDIR /app
ENV CI=true

RUN corepack enable
RUN corepack prepare pnpm@11.8.0 --activate

COPY package.json package.json
COPY pnpm-lock.yaml pnpm-lock.yaml

RUN pnpm install --frozen-lockfile

COPY . .

RUN ./node_modules/.bin/ng build

FROM nginx:alpine

COPY --from=build /app/dist/clovance/browser /usr/share/nginx/html
COPY --from=build /app/default.conf.template /etc/nginx/templates/default.conf.template

# Expose the default nginx port
EXPOSE 8000

CMD ["nginx", "-g", "daemon off;"]
