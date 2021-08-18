FROM mhart/alpine-node:12

# install dependencies
WORKDIR /app
COPY package.json yarn.lock ./
RUN yarn --frozen-lockfile

# Copy all local files into the image.
COPY . .

RUN yarn build

###
# Only copy over the Node pieces we need
# ~> Saves 35MB
###
FROM mhart/alpine-node:slim-12

WORKDIR /app
COPY --from=0 /app .
COPY . .

CMD ["node", "./build"]