# syntax=docker/dockerfile:1

# Create a stage for building the application.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

COPY . /source

WORKDIR /source/ET.Web

# This is the architecture you’re building for, which is passed in by the builder.
# Placing it here allows the previous steps to be cached across architectures.
ARG TARGETARCH

# Build the application.
# Leverage a cache mount to /root/.nuget/packages so that subsequent builds don't have to re-download packages.
# If TARGETARCH is "amd64", replace it with "x64" - "x64" is .NET's canonical name for this and "amd64" doesn't
#   work in .NET 6.0.
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

# If you need to enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md

# Set up the Sqlite database in a subdirectory, with ownership set
# to a non-privileged user.
RUN <<EOSQLITE
dotnet tool restore
mkdir appdata/
dotnet ef database update
mkdir /app/appdata
cp appdata/et.db /app/appdata/
chown -R $APP_UID:$APP_UID /app/appdata
EOSQLITE

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS finalsqlite
WORKDIR /app

# Copy everything needed to run the app from the "build" stage.
COPY --from=build /app .

# Switch to a non-privileged user (defined in the base image) that the app will run under.
# See https://docs.docker.com/go/dockerfile-user-best-practices/
# and https://github.com/dotnet/dotnet-docker/discussions/4764
USER $APP_UID

ENTRYPOINT ["dotnet", "ET.Web.dll"]

# Advertise the mountpoint /app/appdata where the MySQL database
# resides, so that a volume can be mounted there
VOLUME ["/app/appdata"]