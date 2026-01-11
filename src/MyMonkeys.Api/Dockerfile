# syntax=docker/dockerfile:1

FROM golang:1.25-alpine AS build
WORKDIR /src

COPY go.mod ./
RUN go mod download

COPY . .
RUN CGO_ENABLED=0 GOOS=linux go build -o /out/monkeys-service ./

FROM alpine:3.20
WORKDIR /app

COPY --from=build /out/monkeys-service ./monkeys-service
COPY data ./data

ENV PORT=8080
EXPOSE 6001

ENTRYPOINT ["/app/monkeys-service"]
