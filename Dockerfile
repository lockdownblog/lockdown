FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

WORKDIR /src/

COPY Lockdown/ ./

RUN dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false -o /publish/

FROM mcr.microsoft.com/dotnet/runtime:5.0 as runner

COPY --from=builder /publish/Lockdown /usr/local/bin/lockdown

ENTRYPOINT ["lockdown"]

CMD ["run", "-p /site"]
