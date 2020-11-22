CURRENT_VERSION=0.0.0

targets:="win-x64" "linux-x64" "linux-arm" "osx-x64"

init:
	python -m venv ./.venv
	./.venv/bin/pip install tbump

build:
	for i in $(targets); do \
		dotnet publish ./Lockdown/Lockdown.csproj -r $$i -p:PublishSingleFile=true --self-contained false -o ./publish/$$i; \
	done 
	
build-docs:
	dotnet run --project ./Lockdown/Lockdown.csproj -- build -p docs -o _docs

run-docs:
	dotnet run --project ./Lockdown/Lockdown.csproj -- run -p docs -o _docs

docker:
	docker build -t lockdownblog/lockdown:${CURRENT_VERSION} .

clean:
	rm -rf ./publish
