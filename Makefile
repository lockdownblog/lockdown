CURRENT_VERSION=0.0.4

BRANCH := $(shell git rev-parse --abbrev-ref HEAD)
HASH := $(shell git rev-parse HEAD)
TAG := $(shell git tag -l --contains HEAD)

targets:="win-x64" "linux-x64" "linux-arm" "osx-x64"

init:
	if [ -d "./.venv" ]; then \
		echo "Directory exists"; \
	else \
		python -m venv ./.venv; \
	fi
	./.venv/bin/pip install bump2version

check_on_main:
ifeq ($(BRANCH),main)
	echo "You are good to go!"
else
	$(error You are not in the main branch)
endif

build:
	for i in $(targets); do \
		dotnet publish ./Lockdown/Lockdown.csproj -r $$i -p:PublishSingleFile=true --self-contained false -o ./publish/$$i; \
	done 
	
build-docs:
	dotnet run --project ./Lockdown/Lockdown.csproj -- build --root docs --out _docs

run-docs:
	dotnet run --project ./Lockdown/Lockdown.csproj -- run --root docs --out _docs

docker:
	docker build -t lockdownblog/lockdown:${CURRENT_VERSION} .

bump_patch: check_on_main
	./.venv/bin/python -m bumpversion patch --verbose

bump_minor: check_on_main
	./.venv/bin/python -m bumpversion minor --verbose

bump_major: check_on_main
	./.venv/bin/python -m bumpversion major --verbose

clean:
	rm -rf ./publish
