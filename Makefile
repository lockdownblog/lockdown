CURRENT_VERSION=0.0.0

init:
	python -m venv ./.venv
	./.venv/bin/pip install tbump

build:
	for target in "win-x64" "linux-x64" "linux-arm" "osx-x64"; do \
		dotnet publish ./Lockdown/Lockdown.csproj  -r $target -p:PublishSingleFile=true --self-contained false -o ./publish/$target \
    done
	
clean:
	rm -rf ./publish
